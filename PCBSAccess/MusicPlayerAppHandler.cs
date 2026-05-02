using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace PCBSAccess
{
    /// <summary>
    /// Announces the Music Player app.
    /// PlayTrack Postfix → "Now playing: track N, name."
    /// Populate Postfix → announces track count and sets focus to first row.
    /// OnPlayPause Postfix → "Playing." / "Paused."
    /// OnNext/OnPrev Postfix → announces new track name.
    /// Shuffle/Loop/Mute Postfix → announces toggle state.
    /// SetVolume Postfix → announces volume level.
    /// SetTab Postfix → announces active tab name.
    /// Navigation: Up/Down/Home/End to browse list, Enter to play focused track.
    /// </summary>
    public static class MusicPlayerAppHandler
    {
        #region State

        private static MusicPlayerApp _app;
        private static List<MusicPlayerTrackRow> _rows = new List<MusicPlayerTrackRow>();
        private static int _focusIndex = -1;

        // Deferred refresh: read rows one frame after Populate to let Unity process
        // pending Destroy calls before querying GetComponentsInChildren.
        private static bool _needsRefresh = false;
        private static MusicPlayerApp _pendingRefreshApp = null;

        #endregion

        #region Public API

        public static void Reset()
        {
            _app = null;
            _rows.Clear();
            _focusIndex = -1;
            _needsRefresh = false;
            _pendingRefreshApp = null;
            InputRouter.Pop(_ctx);
        }

        #endregion

        #region IInputContext

        private sealed class Context : IInputContext, IHelpContext
        {
            public string ContextName => "MusicPlayer";
            public bool IsActive => _app != null;

            public bool HandleInput()
            {
                if (_rows.Count == 0) return false;

                if (Input.GetKeyDown(KeyCode.DownArrow)) { Navigate(1);  return true; }
                if (Input.GetKeyDown(KeyCode.UpArrow))   { Navigate(-1); return true; }
                if (Input.GetKeyDown(KeyCode.Home))       { NavigateTo(0);               return true; }
                if (Input.GetKeyDown(KeyCode.End))        { NavigateTo(_rows.Count - 1); return true; }
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    if (_focusIndex >= 0 && _focusIndex < _rows.Count)
                        _rows[_focusIndex]?.OnSelect();
                    return true;
                }
                return false;
            }
            public void AnnounceHelp() { ScreenReader.Say(Loc.Get("help_musicplayer")); }
        }

        private static readonly Context _ctx = new Context();

        #endregion

        #region Public API — deferred refresh

        /// <summary>
        /// Applies deferred row refresh. Called from Main.Update each frame.
        /// Must continue to run every frame for the deferred-refresh pattern to work.
        /// Key handling has moved to Context.HandleInput().
        /// </summary>
        public static void Update()
        {
            // Apply deferred row refresh (one frame after Populate)
            if (_needsRefresh && _pendingRefreshApp != null)
            {
                _needsRefresh = false;
                ApplyRefresh(_pendingRefreshApp);
            }
        }

        #region Navigation

        private static void Navigate(int dir)
        {
            if (_rows.Count == 0) return;
            int next = Mathf.Clamp(_focusIndex + dir, 0, _rows.Count - 1);
            if (next == _focusIndex)
            {
                ScreenReader.Say(dir > 0 ? Loc.Get("nav_last_item") : Loc.Get("nav_first_item"));
                return;
            }
            _focusIndex = next;
            AnnounceRow(_rows[_focusIndex]);
        }

        private static void NavigateTo(int index)
        {
            if (_rows.Count == 0) return;
            _focusIndex = Mathf.Clamp(index, 0, _rows.Count - 1);
            AnnounceRow(_rows[_focusIndex]);
        }

        private static void AnnounceRow(MusicPlayerTrackRow row)
        {
            if (row == null) return;
            ScreenReader.Say(row.m_trackName?.text ?? "?");
        }

        #endregion

        #region Helpers

        private static void ApplyRefresh(MusicPlayerApp app)
        {
            _app = app;
            _rows.Clear();
            if (app.m_trackList?.content != null)
            {
                MusicPlayerTrackRow[] arr =
                    app.m_trackList.content.GetComponentsInChildren<MusicPlayerTrackRow>(false);
                if (arr != null)
                    foreach (MusicPlayerTrackRow r in arr)
                        if (r != null) _rows.Add(r);
            }
            _focusIndex = _rows.Count > 0 ? 0 : -1;
            if (_rows.Count > 0)
            {
                ScreenReader.Say(Loc.Get("music_list", _rows.Count));
                InputRouter.Push(_ctx);
            }
            DebugLogger.LogState($"MusicPlayerAppHandler: {_rows.Count} tracks after refresh");
        }

        #endregion

        #region Patches

        [HarmonyPatch(typeof(MusicPlayerApp), "PlayTrack")]
        static class MusicPlayerApp_PlayTrack_Patch
        {
            static void Postfix(int i, string name)
            {
                try
                {
                    if (i == -1)
                    {
                        ScreenReader.Say(Loc.Get("music_no_track"));
                        return;
                    }
                    ScreenReader.Say(Loc.Get("music_playing", i + 1, name));
                    DebugLogger.LogState($"MusicPlayerAppHandler: playing track {i + 1} '{name}'");
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"MusicPlayerApp_PlayTrack_Patch: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(MusicPlayerApp), "Populate")]
        static class MusicPlayerApp_Populate_Patch
        {
            static void Postfix(MusicPlayerApp __instance)
            {
                try
                {
                    // Schedule deferred read — Destroy calls haven't resolved yet
                    _pendingRefreshApp = __instance;
                    _needsRefresh = true;
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"MusicPlayerApp_Populate_Patch: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(MusicPlayerApp), "OnPlayPause")]
        static class MusicPlayerApp_OnPlayPause_Patch
        {
            static void Postfix(MusicPlayerApp __instance)
            {
                try
                {
                    // m_pause is visible when playing; m_play is visible when paused
                    bool playing = __instance.m_pause.gameObject.activeInHierarchy;
                    ScreenReader.Say(playing ? Loc.Get("music_resumed") : Loc.Get("music_paused"));
                }
                catch (Exception ex)
                {
                    DebugLogger.LogWarning($"MusicPlayerApp_OnPlayPause_Patch: {ex.Message}");
                }
            }
        }

        #endregion

        [HarmonyPatch(typeof(MusicPlayerApp), "OnNext")]
        static class MusicPlayerApp_OnNext_Patch
        {
            static void Postfix(MusicPlayerApp __instance)
            {
                try { ScreenReader.Say(__instance.m_trackName?.text ?? Loc.Get("music_no_track")); }
                catch (Exception ex) { DebugLogger.LogWarning($"MusicPlayerApp_OnNext_Patch: {ex.Message}"); }
            }
        }

        [HarmonyPatch(typeof(MusicPlayerApp), "OnPrev")]
        static class MusicPlayerApp_OnPrev_Patch
        {
            static void Postfix(MusicPlayerApp __instance)
            {
                try { ScreenReader.Say(__instance.m_trackName?.text ?? Loc.Get("music_no_track")); }
                catch (Exception ex) { DebugLogger.LogWarning($"MusicPlayerApp_OnPrev_Patch: {ex.Message}"); }
            }
        }

        [HarmonyPatch(typeof(MusicPlayerApp), "Shuffle")]
        static class MusicPlayerApp_Shuffle_Patch
        {
            static void Postfix(MusicPlayerApp __instance)
            {
                try { ScreenReader.Say(Loc.Get(__instance.m_shuffle.isOn ? "music_shuffle_on" : "music_shuffle_off")); }
                catch (Exception ex) { DebugLogger.LogWarning($"MusicPlayerApp_Shuffle_Patch: {ex.Message}"); }
            }
        }

        [HarmonyPatch(typeof(MusicPlayerApp), "Loop")]
        static class MusicPlayerApp_Loop_Patch
        {
            static void Postfix(MusicPlayerApp __instance)
            {
                try { ScreenReader.Say(Loc.Get(__instance.m_loop.isOn ? "music_loop_on" : "music_loop_off")); }
                catch (Exception ex) { DebugLogger.LogWarning($"MusicPlayerApp_Loop_Patch: {ex.Message}"); }
            }
        }

        [HarmonyPatch(typeof(MusicPlayerApp), "Mute")]
        static class MusicPlayerApp_Mute_Patch
        {
            static void Postfix(MusicPlayerApp __instance)
            {
                try { ScreenReader.Say(Loc.Get(__instance.m_mute.isOn ? "music_muted" : "music_unmuted")); }
                catch (Exception ex) { DebugLogger.LogWarning($"MusicPlayerApp_Mute_Patch: {ex.Message}"); }
            }
        }

        [HarmonyPatch(typeof(MusicPlayerApp), "SetVolume")]
        static class MusicPlayerApp_SetVolume_Patch
        {
            static void Postfix(MusicPlayerApp __instance)
            {
                try
                {
                    int pct = Mathf.RoundToInt(__instance.m_volume.value * 100f);
                    ScreenReader.Say(Loc.Get("music_volume", pct));
                }
                catch (Exception ex) { DebugLogger.LogWarning($"MusicPlayerApp_SetVolume_Patch: {ex.Message}"); }
            }
        }

        [HarmonyPatch(typeof(MusicPlayerApp), "SetTab")]
        static class MusicPlayerApp_SetTab_Patch
        {
            static void Postfix(int i)
            {
                try
                {
                    string key = i == 0 ? "music_tab_soundtrack" :
                                 i == 1 ? "music_tab_userfiles" :
                                          "music_tab_internet";
                    ScreenReader.Say(Loc.Get(key));
                }
                catch (Exception ex) { DebugLogger.LogWarning($"MusicPlayerApp_SetTab_Patch: {ex.Message}"); }
            }
        }

        #endregion
    }
}
