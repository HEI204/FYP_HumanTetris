#if UNITY_TMPRO && UNITY_UI

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flexalon;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace FlexalonCopilot.Editor
{
    internal class ChatWindow : EditorWindow
    {
        [SerializeField]
        private ChatLog _chatLog;

        [SerializeField]
        private PrefabSet _prefabSet;

        [SerializeField]
        private List<UnityEngine.Object> _contextObjects = new List<UnityEngine.Object>();

        [SerializeField]
        private Canvas _canvas;
        public Canvas Canvas
        {
            get => _canvas;
            set => _canvas = value;
        }

        [SerializeField]
        private string _promptText;

        [SerializeField]
        private string _errorText;

        [SerializeField]
        private int _undoGroup = 0;

        [NonSerialized]
        private GetVersionResponse _upgradeResponse;

        private GUIStyle _errorStyle;
        private GUIStyle _logStyle;
        private GUIStyle _promptLogStyle;
        private GUIStyle _promptProcessingStyle;
        private GUIStyle _responseStyle;
        private GUIStyle _helloStyle;
        private GUIStyle _promptContextStyle;
        private GUIStyle _chatBoxStyle;
        private GUIStyle _chatBoxAreaStyle;
        private GUIStyle _errorAreaStyle;
        private GUIStyle _statusAreaStyle;
        private GUIStyle _sendButtonStyle;
        private GUIStyle _selectingStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _feedbackStyle;
        private GUIStyle _bgStyle;
        private CancellationTokenSource _cancellationTokenSource;
        private Vector2 _scrollPosition = new Vector2(0, 999999);
        private bool _processing;
        private Canvas _lastCanvas;
        private bool _focusChatBox;
        private string _selectingText = "";

        [MenuItem("Tools/Flexalon Copilot/Chat Window", false, 1)]
        public static void ShowChatWindow()
        {
            ChatWindow window = GetWindow<ChatWindow>(false, "Flexalon Copilot Chat", true);
            window.Show();
        }

        [MenuItem("GameObject/Flexalon Copilot/Set Canvas", false, 9999)]
        public static void SetCanvas()
        {
            var selected = Selection.activeGameObject;
            if (selected != null && selected.TryGetComponent<Canvas>(out var canvas))
            {
                var window = GetWindow<ChatWindow>(false, "Flexalon Copilot Chat", true);
                Undo.RecordObject(window, "Set Canvas");
                window._canvas = canvas;
            }
        }

        [MenuItem("GameObject/Flexalon Copilot/Add To Context", false, 9999)]
        public static void AddGameObjectToPromptContext()
        {
            var selected = Selection.gameObjects;
            if (selected != null)
            {
                foreach (var gameObject in selected)
                {
                    var window = GetWindow<ChatWindow>(false, "Flexalon Copilot Chat", true);
                    Undo.RecordObject(window, "Add GameObject to Context");
                    if (!window._contextObjects.Contains(gameObject))
                    {
                        window._contextObjects.Add(gameObject);
                    }
                }
            }
        }

        private void Initialize()
        {
            if (_bgStyle != null) return;

            FXGUI.StyleFontSize = 13;
            FXGUI.StyleTag = "FlexalonCopilotChatWindow";

            _logStyle = FXGUI.CreateStyle(FXGUI.Gray(225), FXGUI.Gray(10));

            _bgStyle = new GUIStyle();
            _bgStyle = FXGUI.CreateStyle(FXGUI.Gray(225), FXGUI.Gray(45));

            _promptLogStyle = FXGUI.CreateStyle(FXGUI.Gray(225), new Color(0.12f, 0.1f, 0.13f));
            _promptLogStyle.padding = new RectOffset(20, 20, 15, 15);

            _promptProcessingStyle = FXGUI.CreateStyle(FXGUI.Gray(170), new Color(0.12f, 0.1f, 0.13f));
            _promptProcessingStyle.padding = new RectOffset(20, 20, 15, 15);

            _responseStyle = FXGUI.CreateStyle(FXGUI.Gray(200), FXGUI.Gray(10));
            _responseStyle.padding = new RectOffset(20, 20, 15, 15);

            _helloStyle = FXGUI.CreateStyle(FXGUI.Gray(200), new Color(0.05f, 0.05f, 0.1f));
            _helloStyle.padding = new RectOffset(20, 20, 15, 15);

            _promptContextStyle = new GUIStyle();
            _promptContextStyle.padding = new RectOffset(20, 20, 10, 10);

            _chatBoxStyle = FXGUI.CreateStyle(Color.white, new Color(0.06f, 0.05f, 0.075f));
            _chatBoxStyle.margin.top = 1;
            _chatBoxStyle.padding = new RectOffset(10, 10, 10, 10);
            _chatBoxStyle.border = new RectOffset(5, 5, 5, 5);

            _chatBoxAreaStyle = new GUIStyle();
            _chatBoxAreaStyle.padding = new RectOffset(20, 20, 20, 20);
            FXGUI.SetBackgroundColor(_chatBoxAreaStyle, new Color(0.12f, 0.1f, 0.13f));

            _statusAreaStyle = FXGUI.CreateStyle(Color.white, FXGUI.Gray(10));
            _statusAreaStyle.padding = new RectOffset(20, 0, 10, 10);

            _errorAreaStyle = FXGUI.CreateStyle(Color.white, new Color(0.35f, 0.25f, 0.25f));
            _errorAreaStyle.padding = new RectOffset(20, 20, 10, 10);

            _errorStyle = FXGUI.CreateStyle(Color.white);
            _labelStyle = FXGUI.CreateStyle(Color.white);

            _sendButtonStyle = FXGUI.CreateStyle(Color.white, new Color(0.06f, 0.05f, 0.075f));
            _sendButtonStyle.wordWrap = false;
            _sendButtonStyle.stretchWidth = false;
            _sendButtonStyle.margin.top = 1;
            _sendButtonStyle.padding = new RectOffset(10, 10, 10, 10);
            _sendButtonStyle.border = new RectOffset(5, 5, 5, 5);
            _sendButtonStyle.fontStyle = FontStyle.Bold;

            _selectingStyle = FXGUI.CreateStyle(Color.white, new Color(0.06f, 0.05f, 0.075f));
            _selectingStyle.margin.top = 1;
            _selectingStyle.padding = new RectOffset(10, 10, 10, 10);
            _selectingStyle.border = new RectOffset(5, 5, 5, 5);
            _selectingStyle.fontStyle = FontStyle.Italic;

            _feedbackStyle = FXGUI.CreateStyle(Color.white, FXGUI.Gray(10));
            _feedbackStyle.padding.bottom = 10;
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
            Selection.selectionChanged += Repaint;
            EditorApplication.quitting += OnQuitting;
            EditorApplication.playModeStateChanged += ResetStyle;
            _bgStyle = null;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            Selection.selectionChanged -= Repaint;
            EditorApplication.playModeStateChanged -= ResetStyle;
            FXGUI.CleanupBackgroundTextures("FlexalonCopilotChatWindow");
        }

        private void ResetStyle(PlayModeStateChange mode)
        {
            _bgStyle = null;
        }

        private void OnQuitting()
        {
            _undoGroup = 0;
        }

        private void OnUndoRedo()
        {
            _scrollPosition = new Vector2(0, 1000000);
            Repaint();
        }

        private void OnGUI()
        {
            if (Application.isPlaying)
            {
                GUILayout.Label("Flexalon Copilot is currently not available in play mode.", EditorStyles.boldLabel);
                return;
            }

            Initialize();

            FXGUI.Vertical(_bgStyle, () =>
            {
                FXGUI.DisableGroup(_processing, () =>
                {
                    DrawChatLogSelection();
                    DrawPrefabSet();
                });

                EditorGUILayout.Separator();

                if (!_chatLog)
                {
                    GUILayout.Label("Please select a chat log.", EditorStyles.boldLabel);
                    return;
                }

                if (!_prefabSet)
                {
                    GUILayout.Label("Please select a prefab set.", EditorStyles.boldLabel);
                    return;
                }

                DrawPromptEntries();

                FXGUI.DisableGroup(_processing, () =>
                {
                    DrawChatBox();
                    DrawPromptContext();
                });
            });

            if (_focusChatBox)
            {
                Focus();
                GUI.FocusControl("ChatBox");
                _focusChatBox = false;
                Repaint();
            }
        }

        private void RepaintAndScrollDown()
        {
            Repaint();
            _scrollPosition = new Vector2(0, 1000000);
            Repaint();
        }

        private Prompt CreatePrompt(ChatLogEntry entry)
        {
            var prompt = new Prompt();
            prompt.Id = entry.Id;
            var gids = new GameObjectIdMap();

            var lastEntry = _chatLog.Entries.Count > 1 ? _chatLog.Entries[_chatLog.Entries.Count - 2] : null;
            if (lastEntry != null)
            {
                prompt.PreviousId = lastEntry.Id;
            }

            prompt.PromptContext = PromptContextFactory.Create(_canvas.gameObject, _contextObjects, gids);
            var log = new UpdateLog();
            prompt.SceneUpdater = new SceneUpdater(_canvas.gameObject, _prefabSet, log, gids);
            prompt.SceneUpdater.EnableAnimations();
            _cancellationTokenSource = new CancellationTokenSource();
            prompt.CancellationToken = _cancellationTokenSource.Token;
            prompt.OnUpdate += () =>
            {
                UpdateChatEntryResponse(entry, log);
                RepaintAndScrollDown();
            };

            return prompt;
        }

        private void StartNewUndoGroup()
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Flexalon Copilot Prompt");
            _undoGroup = Undo.GetCurrentGroup();
            Undo.RecordObject(this, "Flexalon Copilot Prompt");
        }

        private void AddChatEntry(ChatLogEntry entry)
        {
            Undo.RecordObject(_chatLog, "Flexalon Copilot Prompt");
            _chatLog.Entries.Add(entry);
            RepaintAndScrollDown();
        }

        private void SetProcessing()
        {
            Selection.activeObject = null;
            _promptText = "";
            _errorText = "";
            _upgradeResponse = null;
            _processing = true;
        }

        private void MoveSceneCameraToCanvas()
        {
            if (_canvas)
            {
                if (SceneView.lastActiveSceneView)
                {
                    Selection.activeGameObject = _canvas.gameObject;
                    SceneView.lastActiveSceneView.rotation = _canvas.transform.rotation;
                    SceneView.lastActiveSceneView.FrameSelected();
                    Selection.activeGameObject = null;
                }
            }
        }

        private void SaveAsset(UnityEngine.Object asset)
        {
            var saveIfDirty = typeof(AssetDatabase).GetMethod("SaveAssetIfDirty", new Type[] { typeof(UnityEngine.Object) });
            if (saveIfDirty != null)
            {
                saveIfDirty.Invoke(null, new object[] { asset });
            }
        }

        public void CreateNewChatLog(bool save)
        {
            _chatLog = ScriptableObject.CreateInstance<ChatLog>();
            _undoGroup = 0;
            _errorText = "";

            if (save)
            {
                var time = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var path = Path.Combine("Assets", "FlexalonCopilotChatLogs", "ChatLog_" + time + ".asset");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                AssetDatabase.CreateAsset(_chatLog, path);
                SaveAsset(_chatLog);
                AssetDatabase.Refresh();
            }
        }

        public async Task SendPromptAsync(string prompt)
        {
            ShowChatWindow();
            _promptText = prompt;
            await SendPromptAsync();
        }

        private void CleanupLastError()
        {
            if (!string.IsNullOrEmpty(_errorText) && _chatLog.Entries.Count > 0 && string.IsNullOrEmpty(_chatLog.Entries.Last().Response))
            {
                _chatLog.Entries.RemoveAt(_chatLog.Entries.Count - 1);
            }
        }

        private async Task SendPromptAsync()
        {
            try
            {
                // Select the Hierarchy window to avoid selecting the prompt anymore, which messes up undo/redo.
                EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");

                StartNewUndoGroup();

                CleanupLastError();

                var promptText = _promptText;

                var entry = new ChatLogEntry()
                {
                    Id = Guid.NewGuid().ToString(),
                    Prompt = promptText,
                    Response = ""
                };

                if (!string.IsNullOrEmpty(_selectingText))
                {
                    entry.Prompt += "\n\n" + _selectingText;
                }

                AddChatEntry(entry);

                CreateRootIfNecessary();

                var prompt = CreatePrompt(entry);

                SetProcessing();

                MoveSceneCameraToCanvas();

                await prompt.SendAsync(promptText);

                OnPromptComplete(entry, prompt);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        private void CreateRootIfNecessary(string id = null)
        {
            if (!_canvas)
            {
                var go = new GameObject("Flexalon Canvas");
                go.layer = LayerMask.NameToLayer("UI");
                Undo.RegisterCreatedObjectUndo(go, "Create Canvas");
                _canvas = Flexalon.Flexalon.AddComponent<Canvas>(go);
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                Flexalon.Flexalon.AddComponent<CanvasScaler>(go);
                Flexalon.Flexalon.AddComponent<GraphicRaycaster>(go);
                Flexalon.Flexalon.AddComponent<FlexalonResult>(go);
                var obj = Flexalon.Flexalon.AddComponent<FlexalonObject>(go);
                var flex = Flexalon.Flexalon.AddComponent<FlexalonFlexibleLayout>(go);
                SerializedComponents.ApplyDefaults(flex);
                SerializedComponents.ApplyDefaults(obj);

                if (id != null)
                {
                    var data = Flexalon.Flexalon.AddComponent<FlexalonCopilotData>(go);
                    data.GeneratedId = id;
                }
            }

            _lastCanvas = _canvas;
        }

        private async void SendRepeatAsync()
        {
            try
            {
                var lastChatEntry = _chatLog.Entries.LastOrDefault();
                var rootId = FlexalonCopilotData.GetId(_lastCanvas.gameObject);
                await UndoAsync();

                StartNewUndoGroup();

                lastChatEntry.Response = "";
                AddChatEntry(lastChatEntry);

                CreateRootIfNecessary(rootId);

                var prompt = CreatePrompt(lastChatEntry);
                prompt.PreviousId = _chatLog.Entries.Count > 1 ? _chatLog.Entries[_chatLog.Entries.Count - 2].Id : null;

                SetProcessing();

                MoveSceneCameraToCanvas();

                await prompt.SendRepeatAsync(lastChatEntry.Id);

                OnPromptComplete(lastChatEntry, prompt);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        private void OnPromptComplete(ChatLogEntry entry, Prompt prompt)
        {
            Undo.RecordObject(this, "Flexalon Copilot Prompt");
            _errorText = prompt.ErrorMessage;
            _upgradeResponse = prompt.UpgradeResponse;
            _processing = false;
            _cancellationTokenSource = null;
            _scrollPosition = new Vector2(0, 1000000);
            Undo.CollapseUndoOperations(_undoGroup);

            if (_upgradeResponse != null)
            {
                UndoAsync().ConfigureAwait(false);
            }

            EditorUtility.SetDirty(_chatLog);

            _focusChatBox = true;
            Repaint();
        }

        private void UpdateChatEntryResponse(ChatLogEntry entry, UpdateLog log)
        {
            Undo.RecordObject(_chatLog, "Flexalon Copilot Prompt");
            var response = log.TakeLogs();
            if (!string.IsNullOrWhiteSpace(response))
            {
                if (!string.IsNullOrEmpty(entry.Response))
                {
                    entry.Response += "\n";
                }

                entry.Response += response;
            }
        }

        private async Task UndoAsync()
        {
            // Avoid undo in OnGUI because it calls OnGUI.
            await Task.Delay(1);

            Undo.RevertAllDownToGroup(_undoGroup);
            _undoGroup = 0;
            _errorText = "";
        }

        private void FlexibleSelectableLabel(string text, GUIStyle style, int subWidth = 0)
        {
            GUIContent content = new GUIContent(text);
            float height = style.CalcHeight(content, EditorGUIUtility.currentViewWidth - subWidth);
            EditorGUILayout.SelectableLabel(text, style, GUILayout.Height(height));
        }

        private void DrawChatLogSelection()
        {
            EditorGUILayout.Space();
            FXGUI.Horizontal(() =>
            {
                var newChatLog = (ChatLog)EditorGUILayout.ObjectField("Chat Log", _chatLog, typeof(ChatLog), false);
                if (newChatLog != _chatLog)
                {
                    _chatLog = newChatLog;
                    _undoGroup = 0;
                    RepaintAndScrollDown();
                }

                bool newButton = GUILayout.Button("New", GUILayout.Width(75));
                if (_chatLog == null || newButton)
                {
                    CreateNewChatLog(true);
                }
            });
        }

        private void DrawPrefabSet()
        {
            _prefabSet = (PrefabSet)EditorGUILayout.ObjectField("Prefab Set", _prefabSet, typeof(PrefabSet), false);
            if (_prefabSet == null)
            {
                _prefabSet = PrefabSet.Default;
            }
        }

        private static readonly string _noResponse = "Something went wrong interpretting Copilot's response. Please try again.";

        private void DrawPromptEntries()
        {
            _scrollPosition = FXGUI.Scroll(_scrollPosition, _logStyle, () =>
            {
                GUILayout.FlexibleSpace();
                FlexibleSelectableLabel("<b>Hi there! I can help you generate UI for your Unity project.</b>\n\n" +
                    " • You can ask me to generate something new like a sign in screen, quest panel, or inventory.\n\n" +
                    " • I will generate something simple to start with. Then, we can work together to improve it!\n\n" +
                    " • <b>I can NOT generate scripts or images</b>! To have me insert an image, select it from your project window.",
                    _helloStyle);

                foreach (var entry in _chatLog.Entries.Take(_chatLog.Entries.Count - 1))
                {
                    FlexibleSelectableLabel(entry.Prompt.Trim(), _promptLogStyle);
                    var response = entry.Response.Trim();
                    FlexibleSelectableLabel(!string.IsNullOrEmpty(response) ? response : _noResponse, _responseStyle);
                    DrawFeedback(entry);
                }

                if (_chatLog.Entries.Count > 0)
                {
                    FlexibleSelectableLabel(_chatLog.Entries.Last().Prompt.Trim(), _processing ? _promptProcessingStyle : _promptLogStyle);
                    var newResponse = _chatLog.Entries.Last().Response.Trim();
                    if ((!_processing && string.IsNullOrWhiteSpace(_errorText)) || !string.IsNullOrEmpty(newResponse))
                    {
                        FlexibleSelectableLabel(!string.IsNullOrEmpty(newResponse) ? newResponse : _noResponse, _responseStyle);
                    }

                    if (!_processing && !string.IsNullOrEmpty(newResponse))
                    {
                        DrawFeedback(_chatLog.Entries.Last());
                    }
                }

                DrawProcessing();
                DrawRetry();
                DrawError();
                DrawUpgrade();
            });
        }

        private void DrawFeedback(ChatLogEntry entry)
        {
            FXGUI.Horizontal(_feedbackStyle, () =>
            {
                GUILayout.FlexibleSpace();
                if (FXGUI.ImageButton("d77f7e6041730b8448081abf9df10a90", 32, 32))
                {
                    FeedbackWindow.ShowFeedbackWindow(entry.Id, true);
                }

                if (FXGUI.ImageButton("a2567f3ff7b0ebc4db2d208eaaea7604", 32, 32))
                {
                    FeedbackWindow.ShowFeedbackWindow(entry.Id, false);
                }
            });
        }

        private void DrawPromptContext()
        {
            FXGUI.Vertical(_promptContextStyle, () =>
            {
                _canvas = (Canvas)EditorGUILayout.ObjectField("Canvas", _canvas, typeof(Canvas), true);

                GUILayout.Space(10);

                if (_contextObjects.Any())
                {
                    for (int i = 0; i < _contextObjects.Count; i++)
                    {
                        FXGUI.Horizontal(() =>
                        {
                            _contextObjects[i] = (UnityEngine.Object)EditorGUILayout.ObjectField(_contextObjects[i], typeof(UnityEngine.Object), true);
                            if (GUILayout.Button("Remove", GUILayout.Width(75)))
                            {
                                _contextObjects.RemoveAt(i);
                                i--;
                            }
                        });
                    }
                }

                FXGUI.Horizontal(() =>
                {
                    EditorGUILayout.LabelField("Use assets should I use?");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Clear All", GUILayout.Width(75)))
                    {
                        _canvas = null;
                        _contextObjects.Clear();
                        Selection.activeObject = null;
                    }
                    if (GUILayout.Button("Add", GUILayout.Width(75)))
                    {
                        _contextObjects.Add(null);
                    }
                });
            });
        }

        private void DrawProcessing()
        {
            if (!_processing)
            {
                return;
            }

            FXGUI.Horizontal(_statusAreaStyle, () =>
            {
                GUILayout.Label("Processing...");
                if (GUILayout.Button("Stop", GUILayout.Width(75)))
                {
                    _cancellationTokenSource?.Cancel();
                    _processing = false;
                }
            });

        }

        private void DrawRetry()
        {
            if (_processing || !_lastCanvas || _undoGroup == 0 || _chatLog.Entries.Count == 0)
            {
                return;
            }

            FXGUI.Horizontal(_statusAreaStyle, () =>
            {
                GUILayout.Label("Caution: This will undo everything since the prompt was sent.", _labelStyle);
                if (GUILayout.Button("Retry", GUILayout.Width(68)))
                {
                    UndoAsync().ConfigureAwait(false);
                    _focusChatBox = true;
                }

                #if FLEXALON_COPILOT_TEST
                    if (GUILayout.Button("Repeat", GUILayout.Width(68)))
                    {
                        SendRepeatAsync();
                    }
                #endif
            });

        }

        private void DrawError()
        {
            if (string.IsNullOrWhiteSpace(_errorText))
            {
                return;
            }

            FXGUI.Horizontal(_errorAreaStyle, () =>
            {
                FlexibleSelectableLabel("There was an error: \n" + _errorText, _errorStyle);
            });
        }

        private void DrawUpgrade()
        {
            if (_upgradeResponse != null)
            {
                FXGUI.Horizontal(_errorAreaStyle, () =>
                {
                    FlexibleSelectableLabel($"Flexalon Copilot needs to update to v{_upgradeResponse.version}.", _errorStyle);
                    if (GUILayout.Button("Install Update", GUILayout.Width(125)))
                    {
                        UpdateUtil.UpdateFromUrl(_upgradeResponse.downloadLink);
                        _upgradeResponse = null;
                    }
                });
            }
        }

        private void DrawChatBox()
        {
            FXGUI.Vertical(_chatBoxAreaStyle, () =>
            {
                if (!Api.Instance.IsLoggedIn)
                {
                    GUILayout.Label("Please log in to use Flexalon Copilot.", EditorStyles.boldLabel);
                    if (GUILayout.Button("Log In", GUILayout.Width(75)))
                    {
                        StartScreen.ShowStartScreen();
                    }
                }
                else
                {
                    Event e = Event.current;
                    bool enterPressed = e.type == EventType.KeyDown &&
                        (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) &&
                        e.modifiers != EventModifiers.Shift;

                    if (enterPressed)
                    {
                        e.Use();
                    }

                    // EditorGUILayout.LabelField("Enter a prompt", _labelStyle);

                    FXGUI.Horizontal(() =>
                    {
                        GUI.SetNextControlName("ChatBox");
                        _promptText = GUILayout.TextArea(_promptText, _chatBoxStyle);
                        GUILayout.Space(1);
                        bool buttonPressed = GUILayout.Button("Send", _sendButtonStyle, GUILayout.Width(55));
                        if (!_processing && (buttonPressed || enterPressed) && _promptText?.Trim().Length > 3)
                        {
                            SendPromptAsync().ConfigureAwait(false);
                        }
                    });

                    _selectingText = "";

                    if (_canvas)
                    {
                        var selectedGameObjectsUnderRoot = Selection.gameObjects.Where(go => go != _canvas.gameObject && go.transform.IsChildOf(_canvas.transform)).Select(go => go.name);
                        if (selectedGameObjectsUnderRoot.Any())
                        {
                            _selectingText += "I am selecting gameObjects '" + string.Join("', '", selectedGameObjectsUnderRoot) + "'" + ". ";
                        }
                    }

                    var selectedAssets = Selection.objects.Where(o => PromptContextFactory.IsSupportedAsset(o));
                    if (selectedAssets.Any())
                    {
                        _selectingText += "I am selecting assets '" + string.Join("', '", selectedAssets) + "'" + ".";
                    }

                    if (!string.IsNullOrEmpty(_selectingText))
                    {
                        FXGUI.Horizontal(() =>
                        {
                            EditorGUILayout.LabelField(_selectingText, _selectingStyle);
                            GUILayout.Space(1);
                            if (GUILayout.Button("Clear", _sendButtonStyle, GUILayout.Width(55)))
                            {
                                Selection.activeObject = null;
                            }
                        });
                    }
                }
            });
        }
    }
}

#endif