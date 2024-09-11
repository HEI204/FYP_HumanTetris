#if UNITY_TMPRO && UNITY_UI

using UnityEditor;
using UnityEngine;

namespace FlexalonCopilot.Editor
{
    [CustomEditor(typeof(FlexalonStyleComponent)), CanEditMultipleObjects]
    internal class FlexalonStyleComponentEditor : UnityEditor.Editor
    {
        private SerializedProperty _style;
        private UnityEditor.Editor _styleEditor;

        public void OnEnable()
        {
            _style = serializedObject.FindProperty("_style");
        }

        public void OnDisable()
        {
            if (_styleEditor)
            {
                DestroyImmediate(_styleEditor);
                _styleEditor = null;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_style);

            if (targets.Length == 1)
            {
                if (_style.objectReferenceValue != null)
                {
                    var style = _style.objectReferenceValue as FlexalonStyle;
                    if (style == null || _styleEditor?.target != style)
                    {
                        DestroyImmediate(_styleEditor);
                        _styleEditor = null;
                    }

                    if (style != null && _styleEditor == null)
                    {
                        _styleEditor = CreateEditor(style);
                    }

                    if (_styleEditor != null)
                    {
                        _styleEditor.OnInspectorGUI();
                    }
                }
                else
                {
                    if (GUILayout.Button("Create Inline Style"))
                    {
                        FlexalonStyle.CreateInlineStyle(target as FlexalonStyleComponent);
                    }
                }
            }
            else
            {
                if (_styleEditor)
                {
                    DestroyImmediate(_styleEditor);
                    _styleEditor = null;
                }

                if (GUILayout.Button("Apply Properties"))
                {
                    foreach (var target in targets)
                    {
                        var styleComponent = target as FlexalonStyleComponent;
                        if (styleComponent.Style != null)
                        {
                            AnimatedPropertyOrField.RecordEdits = true;
                            FlexalonStyle.ApplyAllStyles(styleComponent.gameObject);
                            AnimatedPropertyOrField.RecordEdits = false;
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif