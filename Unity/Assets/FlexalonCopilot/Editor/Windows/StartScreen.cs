#if UNITY_TMPRO && UNITY_UI

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FlexalonCopilot.Editor
{
    [InitializeOnLoad]
    internal class StartScreen : EditorWindow
    {
        private static readonly string _account = "https://ai.flexalon.com";
        private static readonly string _discord = "https://discord.gg/VM9cWJ9rjH";
        private static readonly string _docs = "https://ai.flexalon.com/docs?utm_source=fxmenu";

        private static readonly string _showOnStartKey = "FlexalonCopilotMenu_ShowOnStart";
        private static readonly string _versionKey = "FlexalonCopilotMenu_Version";

        private GUIStyle _buttonStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _warningStyle;
        private GUIStyle _versionStyle;
        private GUIStyle _boldStyle;

        private GUIStyle _bgStyle;

        private GUIStyle _paddedSection;

        private LoginWindow _login;

        private static ShowOnStart _showOnStart;

        internal static readonly string WindowTitle = "Flexalon Copilot";
        private static readonly string[] _showOnStartOptions = {
            "Always", "On Update", "Never"
        };

        private Vector2 _scrollPosition;

        private List<string> _changelog = new List<string>();

        private enum ShowOnStart
        {
            Always,
            OnUpdate,
            Never
        }

        static StartScreen()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            EditorApplication.update -= OnEditorUpdate;
            Initialize();
        }

        internal static void Initialize()
        {
            var shownKey = "FlexalonCopilotMenuShown";
            bool alreadyShown = SessionState.GetBool(shownKey, false);
            SessionState.SetBool(shownKey, true);

            var version = WindowUtil.GetVersion();
            var lastVersion = EditorPrefs.GetString(_versionKey, "0.0.0");
            var newVersion = version.CompareTo(lastVersion) > 0;
            if (newVersion)
            {
                EditorPrefs.SetString(_versionKey, version);
                alreadyShown = false;
            }

            _showOnStart = (ShowOnStart)EditorPrefs.GetInt(_showOnStartKey, 0);
            bool showPref = _showOnStart == ShowOnStart.Always ||
                (_showOnStart == ShowOnStart.OnUpdate && newVersion);
            if (!EditorApplication.isPlayingOrWillChangePlaymode && !alreadyShown && showPref)
            {
                ShowStartScreen();
            }
        }

        void OnEnable()
        {
            _bodyStyle = null;
        }

        void OnDisable()
        {
            FXGUI.CleanupBackgroundTextures("FlexalonCopilotStartScreen");
        }

        [MenuItem("Tools/Flexalon Copilot/Start Screen", false, 0)]
        public static void ShowStartScreen()
        {
            StartScreen window = GetWindow<StartScreen>(true, WindowTitle, true);
            window.minSize = new Vector2(800, 600);
            window.maxSize = window.minSize;
            window.Show();
        }

        [MenuItem("Tools/Flexalon Copilot/Check For Updates", false, 2)]
        public async static void CheckForUpdates()
        {
            var response = await Api.Instance.GetVersionAsync();
            var currentVersion = WindowUtil.GetVersion();
            if (response.version != currentVersion)
            {
                if (EditorUtility.DisplayDialog("Flexalon Copilot Update Available",
                    $"A new version of Flexalon Copilot is available ({response.version}). Would you like to install it now?",
                    "Yes", "No"))
                {
                    UpdateUtil.UpdateFromUrl(response.downloadLink);
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Flexalon Copilot Up To Date",
                    $"You are running the latest version of Flexalon Copilot ({response.version}).",
                    "OK");
            }
        }

        [MenuItem("Tools/Flexalon Copilot/Support (Discord)", false, 3)]
        public static void OpenSupport()
        {
            Application.OpenURL(_discord);
        }

        private void InitStyles()
        {
            if (_bodyStyle != null) return;

            FXGUI.StyleFontSize = 14;
            FXGUI.StyleTag = "FlexalonCopilotStartScreen";

            _bodyStyle = FXGUI.CreateStyle(Color.white);
            _bodyStyle.margin.left = 10;
            _bodyStyle.margin.top = 10;
            _bodyStyle.stretchWidth = false;

            _boldStyle = new GUIStyle(_bodyStyle);
            _boldStyle.fontStyle = FontStyle.Bold;
            _boldStyle.fontSize++;

            _warningStyle = new GUIStyle(_bodyStyle);
            _warningStyle.normal.textColor = Color.yellow;

            _buttonStyle = FXGUI.CreateStyle(Color.white, new Color(0.2f, 0.1f, 0.3f, 1.0f));
            _buttonStyle.fontSize = 14;
            _buttonStyle.margin.left = 10;
            _buttonStyle.margin.bottom = 5;
            _buttonStyle.padding = new RectOffset(10, 10, 5, 5);
            _buttonStyle.stretchWidth = false;
            _buttonStyle.alignment = TextAnchor.MiddleLeft;

            _versionStyle = new GUIStyle(EditorStyles.label);
            _versionStyle.padding.right = 10;

            _bgStyle = FXGUI.CreateStyle(Color.white, Color.black);

            _paddedSection = new GUIStyle();
            _paddedSection.padding = new RectOffset(10, 10, 10, 10);

            WindowUtil.CenterOnEditor(this);

            ReadChangeLog();

            Api.Instance.LoginStateChanged += UpdateUserInfo;
            UpdateUserInfo();

            _login = new LoginWindow(_boldStyle, _bodyStyle, _warningStyle);
            _login.StateChanged += Repaint;
        }

        private void UpdateUserInfo()
        {
            if (Api.Instance.IsLoggedIn)
            {
                Api.Instance.RefreshUserInfo(() => Repaint());
            }
        }

        private void LinkButton(string label, string url, GUIStyle style = null, int width = 170)
        {
            if (style == null) style = _buttonStyle;
            var labelContent = new GUIContent(label);
            var position = GUILayoutUtility.GetRect(width, 35, style);
            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            if (GUI.Button(position, labelContent, style))
            {
                Application.OpenURL(url);
            }
        }

        private bool Button(string label, GUIStyle style = null, int width = 170)
        {
            if (style == null) style = _buttonStyle;
            var labelContent = new GUIContent(label);
            var position = GUILayoutUtility.GetRect(width, 35, style);
            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            return GUI.Button(position, labelContent, style);
        }

        private void Bullet(string text)
        {
            FXGUI.Horizontal(() =>
            {
                var ws = 1 + text.IndexOf('-');
                for (int i = 0; i < ws; i++)
                {
                    GUILayout.Space(10);
                }
                GUILayout.Label("•", _bodyStyle);

                GUILayout.Label(text.Substring(ws + 1), _bodyStyle);
            });
        }

        private void ReadChangeLog()
        {
            _changelog.Clear();
            var changelogPath = AssetDatabase.GUIDToAssetPath("b5564bdbd0cbe0c46b337a08b0d07034");
            var changelogAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(changelogPath);
            _changelog = changelogAsset.text.Split('\n')
                .Select(x => Regex.Replace(x.TrimEnd(), @"\*\*(.*?)\*\*", "<b>$1</b>"))
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            var start = _changelog.FindIndex(l => l.StartsWith("## "));
            var end = _changelog.FindIndex(start + 1, l => l.StartsWith("## "));
            if (end < 0) end = _changelog.Count;
            _changelog = _changelog.GetRange(start, end - start);
        }

        private void WhatsNew()
        {
            EditorGUILayout.Space();
            if (Button("Open Flexalon Copilot Chat Window", _buttonStyle, 550))
            {
                EditorApplication.ExecuteMenuItem("Tools/Flexalon Copilot/Chat Window");
                Close();
            }
            EditorGUILayout.Space();

            for (int i = 0; i < _changelog.Count; i++)
            {
                var line = _changelog[i];
                if (line.StartsWith("###"))
                {
                    EditorGUILayout.Space();
                    GUILayout.Label(line.Substring(4), _boldStyle);
                    EditorGUILayout.Space();
                }
                else if (line.StartsWith("##"))
                {
                    EditorGUILayout.Space();
                    GUILayout.Label(line.Substring(3), _boldStyle);
                    EditorGUILayout.Space();
                }
                else
                {
                    Bullet(line);
                    EditorGUILayout.Space();
                }
            }

            EditorGUILayout.Space();
        }

        private void Account()
        {
            if (Api.Instance.UserInfo == null)
            {
                GUILayout.Label("Loading...", _bodyStyle);
                return;
            }

            FXGUI.Horizontal(() =>
            {
                GUILayout.Label($"You are logged in as {Api.Instance.UserInfo?.email}", _bodyStyle);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Logout"))
                {
                    Api.Instance.Logout();
                }
            });

            if (string.IsNullOrWhiteSpace(Api.Instance.UserInfo?.subType))
            {
                EditorGUILayout.Space();
                GUILayout.Label($"You do not have an active subscription. Update your subscription at ai.flexalon.com", _warningStyle);
            }
        }

        private void OnGUI()
        {
            InitStyles();

            FXGUI.Vertical(_bgStyle, () =>
            {
                FXGUI.Horizontal(_paddedSection, () =>
                {
                    WindowUtil.DrawCopilotIcon(128);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Version: " + WindowUtil.GetVersion(), _versionStyle, GUILayout.ExpandHeight(true));
                });

                FXGUI.Horizontal(() =>
                {
                    FXGUI.Vertical(180, () =>
                    {
                        GUILayout.Space(10);
                        GUILayout.Label("Resources", _boldStyle);
                        GUILayout.Space(10);
                        if (Api.Instance.IsLoggedIn)
                        {
                            if (GUILayout.Button("Open Copilot", _buttonStyle, GUILayout.Width(170), GUILayout.Height(35)))
                            {
                                EditorApplication.ExecuteMenuItem("Tools/Flexalon Copilot/Chat Window");
                                Close();
                            }

                            LinkButton("My Account", _account);
                        }

                        LinkButton("Discord Invite", _discord);
                        LinkButton("Documentation", _docs);
                        GUILayout.FlexibleSpace();
                    });

                    FXGUI.Vertical(() =>
                    {
                        _scrollPosition = FXGUI.Scroll(_scrollPosition, () =>
                        {
                            if (Api.Instance.IsLoggedIn)
                            {
                                GUILayout.Label("Thank you for using Flexalon Copilot!", _boldStyle);

                                EditorGUILayout.Space();

                                Account();

                                EditorGUILayout.Space();

                                GUILayout.Label("You're invited to join the Discord community for support and feedback. Let us know how to make Flexalon Copilot better for you!", _bodyStyle);

                                EditorGUILayout.Space();

                                WhatsNew();
                            }
                            else
                            {
                                GUILayout.Label("Let's get started with Flexalon Copilot!", _boldStyle);
                                _login.Draw();
                            }
                        });
                    });

                    EditorGUILayout.Space();
                });

                FXGUI.Horizontal(_paddedSection, () =>
                {
                    GUILayout.Label("Tools/Flexalon Copilot/Start Screen");
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Show On Start: ");
                    var newShowOnStart = (ShowOnStart)EditorGUILayout.Popup((int)_showOnStart, _showOnStartOptions);
                    if (_showOnStart != newShowOnStart)
                    {
                        _showOnStart = newShowOnStart;
                        EditorPrefs.SetInt(_showOnStartKey, (int)_showOnStart);
                    }
                });
            });
        }
    }
}

#endif