#if UNITY_TMPRO && UNITY_UI

using System;
using System.Collections.Generic;
using System.Linq;
using Flexalon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FlexalonCopilot
{
    [Serializable]
    public class FlexalonStyleProperty
    {
        public string Component;
        public string Property;
        public string Value;
    }

    [CreateAssetMenu(fileName = "Style", menuName = "Flexalon Copilot/Style", order = 281)]
    public class FlexalonStyle : ScriptableObject
    {
        [SerializeField]
        private List<FlexalonStyleProperty> _properties = new List<FlexalonStyleProperty>();
        public List<FlexalonStyleProperty> Properties => _properties;

        public bool IsInline = false;

        public static readonly HashSet<(string, string)> InheritedProperties = new HashSet<(string, string)>()
        {
            ("TextMeshProUGUI", "color"),
            ("TextMeshProUGUI", "font"),
            ("TextMeshProUGUI", "fontSize"),
            ("TextMeshProUGUI", "fontStyle"),
            ("TextMeshProUGUI", "alignment"),
            ("TextMeshProUGUI", "overflowMode"),
            ("TextMeshProUGUI", "enableWordWrapping"),
            ("TMP_InputField", "fontAsset"),
            ("TMP_InputField", "pointSize"),
        };

        public static readonly Dictionary<Type, List<string>> StyledProperties = new Dictionary<Type, List<string>>()
        {
            { typeof(FlexalonObject), new List<string> {
                "Width",
                "Height",
                "WidthOfParent",
                "HeightOfParent",
                "WidthType",
                "HeightType",
                "Offset",
                "MarginLeft",
                "MarginTop",
                "MarginRight",
                "MarginBottom",
                "PaddingLeft",
                "PaddingTop",
                "PaddingRight",
                "PaddingBottom",
            } },
            { typeof(FlexalonFlexibleLayout), new List<string> {
                "Direction",
                "Wrap",
                "WrapDirection",
                "HorizontalAlign",
                "VerticalAlign",
                "HorizontalInnerAlign",
                "VerticalInnerAlign",
                "GapType",
                "Gap",
                "WrapGapType",
                "WrapGap",
            } },
            { typeof(TextMeshProUGUI), new List<string> {
                "font",
                "fontSize",
                "fontStyle",
                "color",
                "alignment",
                "overflowMode",
                "enableWordWrapping",
            } },
            { typeof(TMP_InputField), new List<string> {
                "fontAsset",
                "pointSize",
                "characterLimit",
                "contentType",
                "lineType",
                "readOnly",
            } },
            { typeof(Image), new List<string> {
                "sprite",
                "color",
            } },
        };

        public static readonly List<string> StyledComponents = StyledProperties.Keys.Select(x => x.Name).ToList();
        public static readonly Dictionary<string, Type> ComponentNameToType = StyledProperties.Keys.ToDictionary(x => x.Name, x => x);

        private static List<FlexalonStyleComponent> _styleComponents = new List<FlexalonStyleComponent>();

        public static bool IsInherited(Type componentType, string property)
        {
            return IsInherited(componentType.Name, property);
        }

        public static bool IsInherited(string component, string property)
        {
            return InheritedProperties.Contains((component, property));
        }

        public static void ApplyToAll(FlexalonStyle style)
        {
            #if UNITY_EDITOR
                #if UNITY_2021_2_OR_NEWER
                    var prefab = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                #else
                    var prefab = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                #endif
                if (prefab != null)
                {
                    var root = prefab.prefabContentsRoot;
                    ApplyAllStylesRecursively(root.gameObject);
                    UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                    return;
                }
            #endif

            #if UNITY_2023_1_OR_NEWER
                var styleComponents = FindObjectsByType<FlexalonStyleComponent>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            #else
                var styleComponents = FindObjectsOfType<FlexalonStyleComponent>();
            #endif
            var rootGameObjects = new HashSet<GameObject>();
            foreach (var styleComponent in styleComponents)
            {
                if (styleComponent.Style == style)
                {
                    rootGameObjects.Add(styleComponent.gameObject.transform.root.gameObject);
                }
            }

            foreach (var gameObject in rootGameObjects)
            {
                ApplyAllStylesRecursively(gameObject);
            }

            #if UNITY_EDITOR
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            #endif
        }

        public static void ApplyAllStyles(GameObject gameObject)
        {
            ApplyAllStylesRecursively(gameObject.transform.root.gameObject);

            #if UNITY_EDITOR
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            #endif
        }

        public static void ApplyAllStyles(Component component)
        {
            // TODO: Could be more efficient
            ApplyAllStyles(component.gameObject);
        }

        public static bool IsPropertySetByStyle(Component component, string property, bool includeInherited)
        {
            foreach (var styleComponent in GetStyles(component.gameObject))
            {
                var style = styleComponent.Style;
                if (style != null)
                {
                    if (style.Properties.Any(x => x.Component == component.GetType().Name && x.Property == property))
                    {
                        return true;
                    }
                }
            }

            if (includeInherited && IsInherited(component.GetType().Name, property))
            {
                var parent = component.transform.parent;
                while (parent != null)
                {
                    foreach (var styleComponent in GetStyles(parent.gameObject))
                    {
                        var style = styleComponent.Style;
                        if (style != null)
                        {
                            if (style.Properties.Any(x => x.Property == property))
                            {
                                return true;
                            }
                        }
                    }

                    parent = parent.parent;
                }
            }

            return false;
        }

        public static bool TryGetInlineProperty(Component c, string property, Type propertyType, out object value)
        {
            foreach (var style in GetInlineStyles(c.gameObject))
            {
                var prop = style.Style.Properties.Find(x => x.Component == c.GetType().Name && x.Property == property);
                if (prop != null)
                {
                    if (Serialization.TryDeserialize(propertyType, prop.Value, out value))
                    {
                        return true;
                    }
                }
            }

            value = null;
            return false;
        }

        public static FlexalonStyleComponent GetOrCreateInlineStyle(GameObject gameObject, string component, string property)
        {
            var inlineStyle = GetInlineStyle(gameObject, component, property);
            if (inlineStyle != null)
            {
                return inlineStyle;
            }

            var styleComponent = Flexalon.Flexalon.AddComponent<FlexalonStyleComponent>(gameObject);
            CreateInlineStyle(styleComponent);
            return styleComponent;
        }

        public static FlexalonStyleComponent GetOrCreateInlineStyle(GameObject gameObject)
        {
            var inlineStyle = GetInlineStyle(gameObject);
            if (inlineStyle != null)
            {
                return inlineStyle;
            }

            var styleComponent = Flexalon.Flexalon.AddComponent<FlexalonStyleComponent>(gameObject);
            CreateInlineStyle(styleComponent);
            return styleComponent;
        }

        public static void CreateInlineStyle(FlexalonStyleComponent component)
        {
            var newStyle = FlexalonStyle.CreateInstance<FlexalonStyle>();

#if UNITY_EDITOR
            UnityEditor.Undo.RegisterCreatedObjectUndo(newStyle, "Create Inline Style");
            UnityEditor.Undo.RecordObject(component, "Create Inline Style");
#endif
            newStyle.name = $"{component.gameObject.name} Inline Style";
            newStyle.IsInline = true;
            component.Style = newStyle;
        }

        private static List<FlexalonStyleComponent> GetStyles(GameObject go)
        {
            go.GetComponents<FlexalonStyleComponent>(_styleComponents);
            return _styleComponents;
        }

        private static IEnumerable<FlexalonStyleComponent> GetInlineStyles(GameObject go)
        {
            return GetStyles(go).Where(x => x.Style != null && x.Style.IsInline);
        }

        public static List<FlexalonStyleProperty> GetInlineStyleProperties(GameObject gameObject)
        {
            var properties = new List<FlexalonStyleProperty>();
            foreach (var style in GetInlineStyles(gameObject))
            {
                properties.AddRange(style.Style.Properties);
            }

            return properties;
        }

        public static FlexalonStyleComponent GetInlineStyle(GameObject go)
        {
            return GetStyles(go).FirstOrDefault(x => x.Style != null && x.Style.IsInline);
        }

        public static FlexalonStyleComponent GetInlineStyle(GameObject gameObject, string component, string property)
        {
            var inlineStyles = GetInlineStyles(gameObject);

            // First fine an inline style with this particular property.
            foreach (var style in inlineStyles)
            {
                var prop = style.Style.Properties.Find(x => x.Component == component && x.Property == property);
                if (prop != null)
                {
                    return style;
                }
            }

            // If not, any inline style will do.
            return inlineStyles.FirstOrDefault();
        }

        public bool SetProperty(string component, string property, string value)
        {
            if (ComponentNameToType.TryGetValue(component, out var componentType))
            {
                var prop = _properties.Find(x => x.Component == component && x.Property == property);
                if (prop == null)
                {
                    prop = new FlexalonStyleProperty();
                    prop.Component = component;
                    prop.Property = property;
                    _properties.Add(prop);
                }

                bool changed = prop.Value != value;
                prop.Value = value;
                return changed;
            }

            Log.Warning($"Could not find component type {component}.");
            return false;
        }

        public bool ClearProperty(string component, string property)
        {
            return _properties.RemoveAll(x => x.Component == component && x.Property == property) > 0;
        }

        public void Add(string component, string property, string value)
        {
            _properties.Add(new FlexalonStyleProperty
            {
                Component = component,
                Property = property,
                Value = value
            });
        }

        public void Add<T>(string property, object value)
        {
            Add(typeof(T).Name, property, Serialization.Serialize(value));
        }

#if UNITY_EDITOR
        public static void SaveInlineStyle(FlexalonStyle style)
        {
            var path = UnityEditor.AssetDatabase.GetAssetPath(style);
            if (string.IsNullOrEmpty(path))
            {
                path = UnityEditor.EditorUtility.SaveFilePanelInProject("Save Inline Style", style.name, "asset", "Save Inline Style");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }
            }

            UnityEditor.AssetDatabase.CreateAsset(style, path);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif

        private static void ApplyAllStylesRecursively(GameObject gameObject)
        {
            foreach (var style in GetStyles(gameObject))
            {
                style.Style?.Apply(gameObject);
            }

            foreach (Transform child in gameObject.transform)
            {
                ApplyAllStylesRecursively(child.gameObject);
            }
        }

        private void Apply(GameObject gameObject)
        {
            foreach (var prop in _properties)
            {
                ApplyProperty(gameObject, prop);
            }

            var inheritedProperties = _properties.Where(x => IsInherited(x.Component, x.Property)).ToList();
            foreach (Transform child in gameObject.transform)
            {
                ApplyInheritedProperties(child.gameObject, inheritedProperties);
            }
        }

        private static void ApplyInheritedProperties(GameObject gameObject, List<FlexalonStyleProperty> inheritedProps)
        {
            foreach (var prop in inheritedProps)
            {
                ApplyProperty(gameObject, prop);
            }

            foreach (Transform child in gameObject.transform)
            {
                ApplyInheritedProperties(child.gameObject, inheritedProps);
            }
        }

        private static void ApplyProperty(GameObject gameObject, FlexalonStyleProperty styleProp)
        {
            if (!ComponentNameToType.TryGetValue(styleProp.Component, out var componentType))
            {
                return;
            }

            var component = gameObject.GetComponent(componentType);
            if (component == null)
            {
                return;
            }

            var prop = new AnimatedPropertyOrField(componentType, styleProp.Property);
            if (prop.Valid)
            {
                prop.SetValue(component, Serialization.Deserialize(prop.Type, styleProp.Value));
            }
        }
    }
}

#endif