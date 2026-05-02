using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace PCBSAccess
{
    /// <summary>
    /// Accessibility for the Notes app (compact view).
    ///
    /// On open: announces note count + first note title and text.
    /// Left arrow: previous note (clicks m_navigatePrevious).
    /// Right arrow: next note (clicks m_navigateNext).
    /// F1: re-reads the current note.
    /// </summary>
    public static class NotesAppHandler
    {
        #region State

        private static NotesAppCompactView _view;
        private static bool _active;

        // NotesAppCompactView.m_currentNote is private — read via reflection for re-reads.
        private static readonly FieldInfo _fCurrentNote =
            typeof(NotesAppCompactView).GetField("m_currentNote", BindingFlags.NonPublic | BindingFlags.Instance);

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "Notes";
            public bool IsActive => _active;

            public bool HandleInput()
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow))  { NavigatePrev(); return true; }
                if (Input.GetKeyDown(KeyCode.RightArrow)) { NavigateNext(); return true; }
                if (Input.GetKeyDown(KeyCode.F1))         { ReRead();       return true; }
                return false;
            }

            public void AnnounceHelp()
            {
                ScreenReader.Say(Loc.Get("notes_help"));
            }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Patches

        // NotesApp.Start announces count on initial open.
        [HarmonyPatch(typeof(NotesApp), "Start")]
        static class NotesApp_Start_Patch
        {
            static void Postfix(NotesApp __instance)
            {
                try
                {
                    int count = __instance.Cloud?.Notes?.Count ?? 0;
                    ScreenReader.Say(Loc.Get("notes_open", count));
                    DebugLogger.LogState($"NotesAppHandler: {count} notes");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"NotesApp_Start_Patch: {ex.Message}");
                }
            }
        }

        // Compact view activated: push context and announce current note.
        [HarmonyPatch(typeof(NotesAppCompactView), "OnEnable")]
        static class NotesAppCompactView_OnEnable_Patch
        {
            static void Postfix(NotesAppCompactView __instance)
            {
                try
                {
                    _view   = __instance;
                    _active = true;
                    InputRouter.Push(_ctx);
                    AnnounceCurrentNote();
                    ScreenReader.Say(Loc.Get("notes_compact_hint"), interrupt: false);
                    DebugLogger.LogState("NotesAppHandler: compact view opened");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"NotesAppCompactView_OnEnable_Patch: {ex.Message}");
                }
            }
        }

        // Compact view hidden: pop context.
        [HarmonyPatch(typeof(NotesAppCompactView), "OnDisable")]
        static class NotesAppCompactView_OnDisable_Patch
        {
            static void Postfix()
            {
                try
                {
                    _active = false;
                    _view   = null;
                    InputRouter.Pop(_ctx);
                    DebugLogger.LogState("NotesAppHandler: compact view closed");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"NotesAppCompactView_OnDisable_Patch: {ex.Message}");
                }
            }
        }

        #endregion

        #region Public API

        public static void Reset()
        {
            _active = false;
            _view   = null;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region Navigation

        private static void NavigatePrev()
        {
            if (_view == null) return;
            if (_view.m_navigatePrevious != null && _view.m_navigatePrevious.IsInteractable())
            {
                _view.m_navigatePrevious.onClick.Invoke();
                AnnounceCurrentNote();
            }
            else
            {
                ScreenReader.Say(Loc.Get("nav_first_item"));
            }
        }

        private static void NavigateNext()
        {
            if (_view == null) return;
            if (_view.m_navigateNext != null && _view.m_navigateNext.IsInteractable())
            {
                _view.m_navigateNext.onClick.Invoke();
                AnnounceCurrentNote();
            }
            else
            {
                ScreenReader.Say(Loc.Get("nav_last_item"));
            }
        }

        private static void ReRead()
        {
            AnnounceCurrentNote();
        }

        private static void AnnounceCurrentNote()
        {
            if (_view == null) return;
            try
            {
                // Get count from m_noteCount text ("1/3" format)
                string countText = _view.m_noteCount?.text ?? string.Empty;

                string title = _view.m_noteTitle?.text ?? string.Empty;
                string body  = _view.m_noteText?.text  ?? string.Empty;

                if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(body))
                {
                    ScreenReader.Say(Loc.Get("notes_empty"));
                    return;
                }

                string msg = string.IsNullOrEmpty(body)
                    ? $"{countText}. {title}."
                    : $"{countText}. {title}. {body}";

                ScreenReader.Say(msg);
                DebugLogger.LogState($"NotesAppHandler: '{title}'");
            }
            catch (Exception ex)
            {
                DebugLogger.LogWarning($"NotesAppHandler.AnnounceCurrentNote: {ex.Message}");
            }
        }

        #endregion
    }
}
