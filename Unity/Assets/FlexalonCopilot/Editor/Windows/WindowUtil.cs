using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace FlexalonCopilot.Editor
{
    internal static class WindowUtil
    {
        private static string _version;
        private static Texture2D _copilotIcon;

        public static void CenterOnEditor(EditorWindow window)
        {
#if UNITY_2020_1_OR_NEWER
            var main = EditorGUIUtility.GetMainWindowPosition();
            var pos = window.position;
            float w = (main.width - pos.width) * 0.5f;
            float h = (main.height - pos.height) * 0.5f;
            pos.x = main.x + w;
            pos.y = main.y + h;
            window.position = pos;
#endif
        }

        public static readonly string PackageJsonGuid = "65ce65524501cb349ae27bc06fcb00a4";

        public static string GetVersion()
        {
            if (_version == null)
            {
                var projectFilePath = AssetDatabase.GUIDToAssetPath(PackageJsonGuid);
                var lines = File.ReadAllText(projectFilePath);
                var rx = new Regex("\"version\": \"(.*?)\"");
                _version = rx.Match(lines).Groups[1].Value;
            }

            return _version;
        }

        public static void DrawCopilotIcon(float width)
        {
            if (!_copilotIcon)
            {
                var flexalonIconPath = AssetDatabase.GUIDToAssetPath("fd7b678e1a2f0fd478ea5803bd9c0d0b");
                _copilotIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(flexalonIconPath);
            }

            GUILayout.Label(_copilotIcon, GUILayout.Width(width), GUILayout.Height(width * 0.361f));
        }
    }
}