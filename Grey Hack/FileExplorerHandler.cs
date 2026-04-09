using System.Collections.Generic;
using TMPro;
using UI.Dialogs;
using UnityEngine;

namespace GreyHackAccess
{
    /// <summary>
    /// Handler for file explorer keyboard accessibility.
    /// Auto-activates when a file explorer (VentanaFinder) window is focused.
    /// Up/Down navigates files, Enter opens, Backspace goes up a folder,
    /// Alt+Left/Right for history, Alt+Home for home directory.
    /// </summary>
    public class FileExplorerHandler
    {
        #region Fields

        private static FileExplorerHandler _instance;

        private VentanaFinder _activeExplorer;
        private uDialog _activeDialog;
        private int _currentIndex;
        private List<IconoVentana> _files = new();
        private string _lastPath;
        private bool _navigationActive;

        /// <summary>
        /// Debounce to avoid re-announcing when focus just changed.
        /// </summary>
        private float _lastActivateTime;
        private const float ActivateDebounce = 0.3f;

        /// <summary>
        /// Pending update debounce: the game calls UpdateWindow multiple times
        /// during navigation, and early calls have stale path data.
        /// We wait a short time before reading and announcing.
        /// </summary>
        private bool _pendingUpdate;
        private float _pendingUpdateTime;
        private const float UpdateDebounce = 0.15f;

        #endregion

        #region Static Entry

        /// <summary>
        /// Registers this handler instance.
        /// </summary>
        public void Register()
        {
            _instance = this;
        }

        /// <summary>
        /// Called when a file explorer's folder contents are updated.
        /// Patches can call this to refresh the file list.
        /// </summary>
        public static void OnExplorerUpdated(VentanaFinder finder)
        {
            if (_instance == null) return;
            _instance.HandleExplorerUpdated(finder);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Whether file explorer navigation is currently active.
        /// </summary>
        public bool IsActive => _navigationActive;

        #endregion

        #region Public Methods

        /// <summary>
        /// Called every frame by Main.Update().
        /// Returns true if input was consumed.
        /// </summary>
        public bool Update()
        {
            // Check if we should activate/deactivate based on focused window
            CheckFocusedWindow();

            if (!_navigationActive) return false;

            // Verify explorer is still valid
            if (_activeExplorer == null || _activeDialog == null || !_activeDialog.gameObject.activeInHierarchy)
            {
                Deactivate();
                return false;
            }

            // Process pending folder change after debounce
            if (_pendingUpdate && Time.time - _pendingUpdateTime >= UpdateDebounce)
            {
                _pendingUpdate = false;
                ProcessFolderChange();
            }

            return HandleInput();
        }

        /// <summary>
        /// Returns help text for F1 during file explorer navigation.
        /// </summary>
        public string GetHelpText()
        {
            return Loc.Get("explorer_help");
        }

        #endregion

        #region Focus Detection

        private void CheckFocusedWindow()
        {
            // Find the currently focused window from the taskbar
            var taskbar = Object.FindObjectOfType<uDialog_TaskBar>();
            if (taskbar == null)
            {
                if (_navigationActive) Deactivate();
                return;
            }

            uDialog currentTask = taskbar.CurrentTask;
            if (currentTask == null)
            {
                if (_navigationActive) Deactivate();
                return;
            }

            // Already tracking this window
            if (currentTask == _activeDialog && _navigationActive)
            {
                return;
            }

            // Check if the focused window contains a VentanaFinder (but not DesktopFinder)
            VentanaFinder finder = currentTask.GetComponentInChildren<VentanaFinder>();
            if (finder != null && !(finder is DesktopFinder))
            {
                if (!_navigationActive || finder != _activeExplorer)
                {
                    Activate(finder, currentTask);
                }
            }
            else if (_navigationActive)
            {
                Deactivate();
            }
        }

        #endregion

        #region Activation

        private void Activate(VentanaFinder finder, uDialog dialog)
        {
            _activeExplorer = finder;
            _activeDialog = dialog;
            _currentIndex = 0;
            _navigationActive = true;
            _lastActivateTime = Time.time;

            RefreshFileList();

            string path = GetCurrentPath();
            int fileCount = _files.Count;
            ScreenReader.Say(Loc.Get("explorer_focused", path, fileCount));

            if (_files.Count > 0)
            {
                AnnounceCurrentFile();
            }

            DebugLogger.LogState($"FileExplorerHandler: activated at '{path}', {fileCount} items");
        }

        private void Deactivate()
        {
            _navigationActive = false;
            _activeExplorer = null;
            _activeDialog = null;
            _files.Clear();
            _currentIndex = 0;
            _lastPath = null;
        }

        #endregion

        #region Input Handling

        private bool HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Navigate(1);
                return true;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Navigate(-1);
                return true;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ActivateFile();
                return true;
            }

            // Backspace = go up one folder
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                GoUp();
                return true;
            }

            // Alt+Left = back in history
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.LeftArrow))
            {
                GoBack();
                return true;
            }

            // Alt+Right = forward in history
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.RightArrow))
            {
                GoForward();
                return true;
            }

            // Alt+Home = go to home directory
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Home))
            {
                GoHome();
                return true;
            }

            // Space = repeat current file announcement
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AnnounceCurrentFile();
                return true;
            }

            // Shift+F10 = context menu for selected file
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F10))
            {
                OpenContextMenu();
                return true;
            }

            // F2 = rename selected file
            if (Input.GetKeyDown(KeyCode.F2))
            {
                RenameFile();
                return true;
            }

            // Delete = delete selected file
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                DeleteFile();
                return true;
            }

            return false;
        }

        #endregion

        #region Navigation

        private void Navigate(int direction)
        {
            RefreshFileList();

            if (_files.Count == 0)
            {
                ScreenReader.Say(Loc.Get("explorer_empty"));
                return;
            }

            int newIndex = _currentIndex + direction;

            if (newIndex >= _files.Count || newIndex < 0)
            {
                // Stop at boundary, do not wrap
                return;
            }

            _currentIndex = newIndex;

            // Select the icon in the explorer so context menu works
            SelectCurrentIcon();
            AnnounceCurrentFile();
        }

        private void ActivateFile()
        {
            if (_files.Count == 0 || _currentIndex < 0 || _currentIndex >= _files.Count) return;

            IconoVentana icon = _files[_currentIndex];
            if (icon == null) return;

            string name = icon.GetNombre();
            FileSystem.TipoOpenFile tipo = icon.GetTipoOpenFile();

            if (tipo == FileSystem.TipoOpenFile.Folder)
            {
                ScreenReader.Say(Loc.Get("explorer_opening_folder", name));
            }
            else
            {
                ScreenReader.Say(Loc.Get("explorer_opening_file", name));
            }

            DebugLogger.LogState($"FileExplorerHandler: opening '{name}'");

            // Select the icon first
            _activeExplorer.SelectIcon(icon);

            // Simulate double-click by calling OnClickFichero twice
            icon.OnClickFichero(); // first click (select)
            icon.OnClickFichero(); // second click (open)
        }

        private void GoUp()
        {
            if (_activeExplorer == null) return;

            ScreenReader.Say(Loc.Get("explorer_going_up"));
            _activeExplorer.PulsadoUp();
            _currentIndex = 0;
            DebugLogger.LogState("FileExplorerHandler: going up");
        }

        private void GoBack()
        {
            if (_activeExplorer == null) return;

            ScreenReader.Say(Loc.Get("explorer_going_back"));
            _activeExplorer.PulsadoBack();
            _currentIndex = 0;
            DebugLogger.LogState("FileExplorerHandler: going back");
        }

        private void GoForward()
        {
            if (_activeExplorer == null) return;

            ScreenReader.Say(Loc.Get("explorer_going_forward"));
            _activeExplorer.PulsadoForward();
            _currentIndex = 0;
            DebugLogger.LogState("FileExplorerHandler: going forward");
        }

        private void GoHome()
        {
            if (_activeExplorer == null) return;

            ScreenReader.Say(Loc.Get("explorer_going_home"));
            _activeExplorer.PulsadoHome();
            _currentIndex = 0;
            DebugLogger.LogState("FileExplorerHandler: going home");
        }

        #endregion

        #region File Operations

        private void OpenContextMenu()
        {
            if (_files.Count == 0 || _currentIndex < 0 || _currentIndex >= _files.Count)
            {
                ScreenReader.Say(Loc.Get("explorer_no_selection"));
                return;
            }

            IconoVentana icon = _files[_currentIndex];
            if (icon == null) return;

            // Select the icon so context menu targets it
            _activeExplorer.SelectIcon(icon);

            // Get the contextual menu and open it with the icon's options
            var contextMenu = icon.transform.root.GetComponentInChildren<ContextualMenu>();
            if (contextMenu == null) return;

            contextMenu.SetVentana(_activeExplorer);

            // Get options from the icon's InteractableContextual
            var interactable = icon.GetComponent<InteractableContextual>();
            if (interactable != null && interactable.options != null)
            {
                ControlContextualMenus.Singleton.ClearMenus();
                contextMenu.OpenMenu(interactable.options);
                // ContextMenuHandler takes over navigation from here via Harmony patch
            }
        }

        private void RenameFile()
        {
            if (_files.Count == 0 || _currentIndex < 0 || _currentIndex >= _files.Count) return;

            IconoVentana icon = _files[_currentIndex];
            if (icon == null) return;

            _activeExplorer.SelectIcon(icon);

            // Enable the input field for renaming
            var inputField = icon.GetComponentInChildren<TMP_InputField>();
            if (inputField != null)
            {
                inputField.enabled = true;
                inputField.interactable = true;
                inputField.ActivateInputField();
                ScreenReader.Say(Loc.Get("explorer_renaming", icon.GetNombre()));
            }
        }

        private void DeleteFile()
        {
            if (_files.Count == 0 || _currentIndex < 0 || _currentIndex >= _files.Count) return;

            IconoVentana icon = _files[_currentIndex];
            if (icon == null) return;

            _activeExplorer.SelectIcon(icon);

            string name = icon.GetNombre();
            ScreenReader.Say(Loc.Get("explorer_deleting", name));
            DebugLogger.LogState($"FileExplorerHandler: deleting '{name}'");

            // Trigger delete via context menu action
            var contextMenu = icon.transform.root.GetComponentInChildren<ContextualMenu>();
            if (contextMenu != null)
            {
                contextMenu.SetVentana(_activeExplorer);
                ControlContextualMenus.Singleton.ClearMenus();
                var deleteOptions = new List<OpcionContextual.Opciones> { OpcionContextual.Opciones.Delete };
                contextMenu.OpenMenu(deleteOptions);

                // Find and click the delete option
                foreach (Transform child in contextMenu.transform)
                {
                    var option = child.GetComponent<OpcionContextual>();
                    if (option != null)
                    {
                        option.OptionSelected();
                        break;
                    }
                }
            }
        }

        #endregion

        #region Announcements

        private void AnnounceCurrentFile()
        {
            if (_files.Count == 0 || _currentIndex < 0 || _currentIndex >= _files.Count) return;

            IconoVentana icon = _files[_currentIndex];
            if (icon == null) return;

            string name = icon.GetNombre();
            string typeDesc = GetFileTypeDescription(icon);
            string announcement = Loc.Get("explorer_file_item", _currentIndex + 1, _files.Count, name, typeDesc);
            ScreenReader.Say(announcement);
        }

        #endregion

        #region Explorer Update Callback

        private void HandleExplorerUpdated(VentanaFinder finder)
        {
            if (!_navigationActive || finder != _activeExplorer) return;

            // Schedule a debounced update: the game fires UpdateWindow multiple times
            // during navigation, and early calls have stale path data in barraDir.
            _pendingUpdate = true;
            _pendingUpdateTime = Time.time;
        }

        /// <summary>
        /// Processes folder change after debounce timer expires.
        /// Reads the current path and announces if it actually changed.
        /// </summary>
        private void ProcessFolderChange()
        {
            if (_activeExplorer == null) return;

            RefreshFileList();

            string path = GetCurrentPath();
            if (path != _lastPath)
            {
                _lastPath = path;
                _currentIndex = 0;
                ScreenReader.Say(Loc.Get("explorer_folder_changed", path, _files.Count));

                if (_files.Count > 0)
                {
                    AnnounceCurrentFile();
                }
                else
                {
                    ScreenReader.SayQueued(Loc.Get("explorer_empty"));
                }

                DebugLogger.LogState($"FileExplorerHandler: folder changed to '{path}', {_files.Count} items");
            }
        }

        #endregion

        #region Helpers

        private void RefreshFileList()
        {
            _files.Clear();

            if (_activeExplorer == null) return;

            List<GameObject> objects = _activeExplorer.GetObjetosActuales();
            if (objects == null) return;

            foreach (var obj in objects)
            {
                if (obj == null || !obj.activeInHierarchy) continue;

                IconoVentana icon = obj.GetComponent<IconoVentana>();
                if (icon != null)
                {
                    _files.Add(icon);
                }
            }

            // Clamp index if list changed
            if (_currentIndex >= _files.Count)
            {
                _currentIndex = _files.Count > 0 ? _files.Count - 1 : 0;
            }
        }

        private void SelectCurrentIcon()
        {
            if (_files.Count == 0 || _currentIndex < 0 || _currentIndex >= _files.Count) return;

            IconoVentana icon = _files[_currentIndex];
            if (icon != null && _activeExplorer != null)
            {
                _activeExplorer.SelectIcon(icon);
            }
        }

        private string GetCurrentPath()
        {
            if (_activeExplorer == null) return "";

            var barraDir = ReflectionHelper.GetPrivateField<TMP_Text>(_activeExplorer, "barraDir");
            if (barraDir == null)
            {
                // barraDir is public on VentanaFinder
                barraDir = _activeExplorer.barraDir;
            }

            return barraDir != null ? barraDir.text : "";
        }

        private static string GetFileTypeDescription(IconoVentana icon)
        {
            if (icon == null) return "";

            FileSystem.TipoOpenFile tipo = icon.GetTipoOpenFile();
            switch (tipo)
            {
                case FileSystem.TipoOpenFile.Folder:
                    return Loc.Get("explorer_type_folder");
                case FileSystem.TipoOpenFile.Text:
                    FileSystem.SubTipoFile subTipo = icon.GetSubTipoFile();
                    if (subTipo == FileSystem.SubTipoFile.ScriptSource)
                        return Loc.Get("explorer_type_source");
                    if (subTipo == FileSystem.SubTipoFile.Log)
                        return Loc.Get("explorer_type_log");
                    if (subTipo == FileSystem.SubTipoFile.Image)
                        return Loc.Get("explorer_type_image");
                    if (subTipo == FileSystem.SubTipoFile.PDF)
                        return Loc.Get("explorer_type_pdf");
                    return Loc.Get("explorer_type_text");
                case FileSystem.TipoOpenFile.Binary:
                    FileSystem.Fichero fichero = icon.GetFichero();
                    if (fichero != null && fichero.IsEjecutable())
                        return Loc.Get("explorer_type_program");
                    return Loc.Get("explorer_type_binary");
                default:
                    return Loc.Get("explorer_type_file");
            }
        }

        #endregion
    }
}
