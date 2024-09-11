#if UNITY_TMPRO && UNITY_UI

using UnityEditor;
using UnityEngine;

namespace FlexalonCopilot.Editor
{
    internal class FeedbackWindow : EditorWindow
    {
        private string _promptId;
        private bool _upvote;
        private string _feedback;

        private GUIStyle _textAreaStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _buttonStyle;

        public static void ShowFeedbackWindow(string promptId, bool upvote)
        {
            var window = GetWindow<FeedbackWindow>(true, "Feedback", true);
            window.Show();
            WindowUtil.CenterOnEditor(window);

            window._promptId = promptId;
            window._upvote = upvote;
            window.minSize = new Vector2(800, 300);
        }

        private void Initialize()
        {
            if (_textAreaStyle != null) return;

            _textAreaStyle = new GUIStyle(EditorStyles.textArea);
            _textAreaStyle.fontSize = 16;
            _textAreaStyle.padding = new RectOffset(10, 10, 10, 0);

            _titleStyle = new GUIStyle(EditorStyles.boldLabel);
            _titleStyle.fontSize = 16;
            _titleStyle.padding = new RectOffset(10, 0, 10, 0);

            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.fontSize = 16;
            _buttonStyle.margin = new RectOffset(10, 10, 10, 10);
        }

        void OnGUI()
        {
            Initialize();

            FXGUI.Vertical(() =>
            {
                FXGUI.Horizontal(() =>
                {
                    FXGUI.Image(_upvote ? "d77f7e6041730b8448081abf9df10a90" : "a2567f3ff7b0ebc4db2d208eaaea7604", 40, 40);
                    GUILayout.Label("Provide additional feedback", _titleStyle);
                });

                GUILayout.Space(10);

                _feedback = EditorGUILayout.TextArea(_feedback, _textAreaStyle, GUILayout.ExpandHeight(true));

                GUILayout.Space(10);

                FXGUI.Horizontal(() =>
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Submit", _buttonStyle, GUILayout.Width(100), GUILayout.Height(30)))
                    {
                        SendFeedback();
                        Close();
                    }
                });
            });
        }

        void OnEnable()
        {
            _textAreaStyle = null;
        }

        private async void SendFeedback()
        {
            await Api.Instance.SendFeedbackAsync(_promptId, _upvote ? 1 : -1, _feedback);
        }
    }
}

#endif