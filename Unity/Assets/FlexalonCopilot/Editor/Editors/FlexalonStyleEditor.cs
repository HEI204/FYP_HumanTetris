#if UNITY_TMPRO && UNITY_UI

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FlexalonCopilot.Editor
{
    [CustomEditor(typeof(FlexalonStyle))]
    internal class FlexalonStyleEditor : UnityEditor.Editor
    {
        private SerializedProperty _properties;

        private Type _componentType;
        private List<string> _propertyOptions;

        public void OnEnable()
        {
            _properties = serializedObject.FindProperty("_properties");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var style = target as FlexalonStyle;

            EditorGUILayout.Space();

            if (_properties.arraySize > 0)
            {
                FXGUI.Horizontal(() =>
                {
                    if (style.IsInline && GUILayout.Button("Save Style to Asset", GUILayout.Width(150)))
                    {
                        FlexalonStyle.SaveInlineStyle(style);
                    }

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Apply Properties", GUILayout.Width(120)))
                    {
                        AnimatedPropertyOrField.RecordEdits = true;
                        FlexalonStyle.ApplyToAll(style);
                        AnimatedPropertyOrField.RecordEdits = false;
                    }
                });
            }

            EditorGUILayout.Space();

            for (int i = 0; i < _properties.arraySize; i++)
            {
                DrawItem(i);
            }

            EditorGUILayout.Space();

            FXGUI.Horizontal(() =>
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Add Property", GUILayout.Width(100)))
                {
                    _properties.InsertArrayElementAtIndex(_properties.arraySize);
                }
            });

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawItem(int i)
        {
            FXGUI.Horizontal(() =>
            {
                var item = _properties.GetArrayElementAtIndex(i);
                var component = item.FindPropertyRelative("Component");
                var property = item.FindPropertyRelative("Property");
                var value = item.FindPropertyRelative("Value");

                DrawComponentSelector(component, property, value);
                DrawPropertySelector(component, property, value);
                DrawValueEditor(component, property, value);
                DrawPropertyButtons(i);
            });
        }

        private void DrawComponentSelector(SerializedProperty component, SerializedProperty propertyName, SerializedProperty value)
        {
            if (EditorGUILayout.DropdownButton(new GUIContent(ObjectNames.NicifyVariableName(component.stringValue)), FocusType.Keyboard, GUILayout.Width(120)))
            {
                var menu = new GenericMenu();
                foreach (var componentType in FlexalonStyle.StyledComponents)
                {
                    menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(componentType)), false, () =>
                    {
                        component.stringValue = componentType;
                        propertyName.stringValue = "";
                        value.stringValue = "";
                        serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.ShowAsContext();
            }
        }

        private void DrawPropertySelector(SerializedProperty component, SerializedProperty propertyName, SerializedProperty value)
        {
            if (component.stringValue == "")
            {
                GUILayout.FlexibleSpace();
                return;
            }

            if (EditorGUILayout.DropdownButton(new GUIContent(ObjectNames.NicifyVariableName(propertyName.stringValue)), FocusType.Keyboard, GUILayout.Width(100)))
            {
                var menu = new GenericMenu();

                if (_propertyOptions == null)
                {
                    _componentType = FlexalonStyle.ComponentNameToType[component.stringValue];
                    _propertyOptions = FlexalonStyle.StyledProperties[_componentType];
                }

                foreach (var option in _propertyOptions)
                {
                    var componentType = _componentType;
                    menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(option)), false, () =>
                    {
                        propertyName.stringValue = option;
                        var propertyInfo = componentType.GetProperty(option, BindingFlags.Instance | BindingFlags.Public);
                        var fieldInfo = componentType.GetField(option, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        var type = propertyInfo != null ? propertyInfo.PropertyType : fieldInfo.FieldType;
                        value.stringValue = "";
                        serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.ShowAsContext();
            }
            else
            {
                _propertyOptions = null;
                _componentType = null;
            }
        }

        private void DrawValueEditor(SerializedProperty component, SerializedProperty property, SerializedProperty value)
        {
            var c = component.stringValue;
            var p = property.stringValue;
            var v = value.stringValue;

            if (string.IsNullOrEmpty(p) || !FlexalonStyle.ComponentNameToType.TryGetValue(c, out var componentType))
            {
                GUILayout.FlexibleSpace();
                return;
            }

            switch (PropertyOrField.GetType(componentType, p).Name)
            {
                case "Color":
                    value.stringValue = Serialization.Serialize(EditorGUILayout.ColorField(Serialization.DeserializeOrDefault<Color>(v)));
                    break;
                case "Boolean":
                    value.stringValue = Serialization.Serialize(EditorGUILayout.Toggle(Serialization.DeserializeOrDefault<bool>(v)));
                    break;
                case "Single":
                    value.stringValue = Serialization.Serialize(EditorGUILayout.FloatField(Serialization.DeserializeOrDefault<float>(v)));
                    break;
                case "Int32":
                    value.stringValue = Serialization.Serialize(EditorGUILayout.IntField(Serialization.DeserializeOrDefault<int>(v)));
                    break;
                case "String":
                    value.stringValue = Serialization.Serialize(EditorGUILayout.TextField(Serialization.DeserializeOrDefault<string>(v)));
                    break;
                case "Vector2":
                    value.stringValue = Serialization.Serialize(EditorGUILayout.Vector2Field("", Serialization.DeserializeOrDefault<Vector2>(v)));
                    break;
                case "Vector3":
                    value.stringValue = Serialization.Serialize(EditorGUILayout.Vector3Field("", Serialization.DeserializeOrDefault<Vector3>(v)));
                    break;
                case "Vector4":
                    value.stringValue = Serialization.Serialize(EditorGUILayout.Vector4Field("", Serialization.DeserializeOrDefault<Vector4>(v)));
                    break;
                case "Quaternion":
                    value.stringValue = Serialization.Serialize(EditorGUILayout.Vector4Field("", Serialization.DeserializeOrDefault<Vector4>(v)));
                    break;
                case "Rect":
                    value.stringValue = Serialization.Serialize(EditorGUILayout.RectField(Serialization.DeserializeOrDefault<Rect>(v)));
                    break;
                case "":
                    GUILayout.FlexibleSpace();
                    break;
                case "FontStyles":
                    value.stringValue = Serialization.Serialize(EditorGUILayout.EnumFlagsField(Serialization.DeserializeOrDefault<TMPro.FontStyles>(v)));
                    break;
                case "TextAlignmentOptions":
                    value.stringValue = Serialization.Serialize(EditorGUILayout.EnumFlagsField(Serialization.DeserializeOrDefault<TMPro.TextAlignmentOptions>(v)));
                    break;
                case "TMP_FontAsset":
                    value.stringValue = Serialization.Serialize(EditorGUILayout.ObjectField(Serialization.DeserializeOrDefault<TMPro.TMP_FontAsset>(v), typeof(TMPro.TMP_FontAsset), false));
                    break;
                case "Sprite":
                    value.stringValue = Serialization.Serialize(EditorGUILayout.ObjectField(Serialization.DeserializeOrDefault<Sprite>(v), typeof(Sprite), false));
                    break;
                default:
                    EditorGUILayout.LabelField(value.stringValue);
                    break;
            }
        }

        private void DrawPropertyButtons(int i)
        {
            if (GUILayout.Button("^", GUILayout.Width(20)))
            {
                _properties.MoveArrayElement(i, i - 1);
            }
            if (GUILayout.Button("v", GUILayout.Width(20)))
            {
                _properties.MoveArrayElement(i, i + 1);
            }
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                _properties.DeleteArrayElementAtIndex(i);
            }
        }
    }
}

#endif