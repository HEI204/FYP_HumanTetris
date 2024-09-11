#if UNITY_TMPRO && UNITY_UI

using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace FlexalonCopilot.Editor
{
    internal class LoginWindow
    {
        public event Action StateChanged;

        private enum State
        {
            Email,
            Code,
            Verifying,
            Done,
            Error
        }

        private string _email;
        private string _code;
        private string _errorMessage = "Something went wrong. Please try again later.";
        private State _state = State.Email;
        private bool _agreeToTerms;
        private bool _marketing;

        private GUIStyle _boldStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _warningStyle;
        private GUIStyle _inputStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _checkboxStyle;

        private GUIStyle _linkStyle;
        private bool _enterPressed;
        private string _emailForCode;

        private Regex _emailRegex;

        public LoginWindow(GUIStyle boldStyle, GUIStyle bodyStyle, GUIStyle warningStyle)
        {
            _boldStyle = boldStyle;
            _bodyStyle = bodyStyle;
            _warningStyle = warningStyle;

            _inputStyle = new GUIStyle(EditorStyles.textField);
            _inputStyle.fontSize = 20;
            _inputStyle.margin = new RectOffset(10, 10, 10, 10);
            _inputStyle.padding = new RectOffset(10, 10, 0, 0);
            _inputStyle.alignment = TextAnchor.MiddleLeft;

            _buttonStyle = new GUIStyle(_bodyStyle);
            _buttonStyle.fontSize = 14;
            _buttonStyle.margin.bottom = 5;
            _buttonStyle.padding.top = 5;
            _buttonStyle.padding.left = 10;
            _buttonStyle.padding.right = 10;
            _buttonStyle.padding.bottom = 5;
            _buttonStyle.hover.background = Texture2D.grayTexture;
            _buttonStyle.hover.textColor = Color.white;
            _buttonStyle.active.background = Texture2D.grayTexture;
            _buttonStyle.active.textColor = Color.white;
            _buttonStyle.focused.background = Texture2D.grayTexture;
            _buttonStyle.focused.textColor = Color.white;
            _buttonStyle.normal.background = Texture2D.grayTexture;
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.wordWrap = false;
            _buttonStyle.stretchWidth = false;

            _checkboxStyle = new GUIStyle(EditorStyles.label);
            _checkboxStyle.wordWrap = true;
            _checkboxStyle.fontSize = 14;
            _checkboxStyle.stretchWidth = false;
            _checkboxStyle.richText = true;

            _linkStyle = new GUIStyle(EditorStyles.label);
            _linkStyle.fontSize = 14;
            _linkStyle.stretchWidth = false;
            ColorUtility.TryParseHtmlString("#21a6f0", out var linkColor);
            _linkStyle.normal.textColor = linkColor;

            _emailRegex = new Regex(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);

        }

        public void Draw()
        {
            _enterPressed = Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);
            if (_enterPressed)
            {
                Event.current.Use();
            }

            switch (_state)
            {
                case State.Email:
                    EmailState();
                    break;
                case State.Code:
                    CodeState();
                    break;
                case State.Verifying:
                    VeryfyingState();
                    break;
                case State.Done:
                    DoneState();
                    break;
                case State.Error:
                    ErrorState();
                    break;
            }
        }

        private void SetState(State state)
        {
            _state = state;
            _email = "";
            _code = "";
            StateChanged?.Invoke();
        }

        private void EmailState()
        {
            GUILayout.Label("Email", _bodyStyle);
            _email = EditorGUILayout.TextField(_email, _inputStyle, GUILayout.Height(40));
            GUILayout.Label("We will send a one-time code to this email.", _bodyStyle);
            GUILayout.Label("During this Early Access period, your conversation data with Copilot may be used to improve this service.", _bodyStyle);
            EditorGUILayout.Space();
            _agreeToTerms = FXGUI.Checkbox(_agreeToTerms, () =>
            {
               GUILayout.Label("I agree to Flexalon Copilot", _checkboxStyle);
               FXGUI.Link("Terms of Use", "https://ai.flexalon.com/terms", _linkStyle);
               GUILayout.Label("and have read the", _checkboxStyle);
               FXGUI.Link("Privacy Policy", "https://ai.flexalon.com/privacy", _linkStyle);
            });
            EditorGUILayout.Space();
            _marketing = FXGUI.Checkbox(_marketing, "Let me know about new features and updates to Virtual Maker products.", _checkboxStyle);
            EditorGUILayout.Space();

            bool canContinue = _email != null && _emailRegex.IsMatch(_email) && _agreeToTerms;
            FXGUI.DisableGroup(!canContinue, () =>
            {
                GUI.SetNextControlName("LoginNextButton");
                if ((_enterPressed || GUILayout.Button("Next", _buttonStyle)) && canContinue)
                {
                    GUI.FocusControl("LoginNextButton");
                    _emailForCode = _email;
                    SendEmail();
                    SetState(State.Code);
                }
            });
        }

        private async void SendEmail()
        {
            try
            {
                await Api.Instance.SignupAsync(_email);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                _errorMessage = e.Message;
                SetState(State.Error);
            }
        }

        private void CodeState()
        {
            GUILayout.Label("Please enter the code you received by email", _bodyStyle);
            _code = EditorGUILayout.TextField(_code, _inputStyle, GUILayout.Height(40));
            FXGUI.Horizontal(() =>
            {
                if (GUILayout.Button("Back", _buttonStyle))
                {
                    SetState(State.Email);
                }

                GUI.SetNextControlName("LoginCodeButton");
                if (_enterPressed || GUILayout.Button("Login", _buttonStyle))
                {
                    GUI.FocusControl("LoginCodeButton");
                    SendLogin();
                }
            });
        }

        private async void SendLogin()
        {
            try
            {
                var sendCode = _code;
                SetState(State.Verifying);
                await Api.Instance.LoginAsync(_emailForCode, sendCode);
                if (_marketing)
                {
                    await Api.Instance.UpdateUserAsync(true);
                }
                SetState(State.Done);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                _errorMessage = e.Message;
                SetState(State.Error);
            }
        }

        private void VeryfyingState()
        {
            GUILayout.Label("Verifying your code...", _bodyStyle);
        }

        private void DoneState()
        {
            GUILayout.Label("You are now logged in", _bodyStyle);
        }

        private void ErrorState()
        {
            GUILayout.Label(_errorMessage, _warningStyle);
            EditorGUILayout.Space();
            if (GUILayout.Button("Try again", _buttonStyle))
            {
                SetState(State.Email);
            }
        }
    }
}

#endif