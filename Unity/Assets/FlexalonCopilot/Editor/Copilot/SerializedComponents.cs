#if UNITY_TMPRO && UNITY_UI

using Flexalon;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace FlexalonCopilot.Editor
{
    internal static class SerializedComponents
    {
        public static readonly Dictionary<Type, Dictionary<string, object>> SerializedTypes = new Dictionary<Type, Dictionary<string, object>>()
        {
            { typeof(RectTransform), new Dictionary<string, object> {
                { "offsetMin", new Vector2(0f, 0f) },
                { "offsetMax", new Vector2(0f, 0f) },
                { "anchorMin", new Vector2(0.5f, 0.5f) },
                { "anchorMax", new Vector2(0.5f, 0.5f) },
                { "pivot", new Vector2(0.5f, 0.5f) },
                { "anchoredPosition", new Vector2(0, 0) }
            } },
            { typeof(FlexalonPanel), new Dictionary<string, object> {
            } },
            { typeof(FlexalonObject), new Dictionary<string, object> {
                { "Width", 0f },
                { "Height", 0f },
                { "WidthOfParent", 1f },
                { "HeightOfParent", 1f },
                { "WidthType", SizeType.Component },
                { "HeightType", SizeType.Component },
                { "Offset", Vector3.zero },
                { "MarginLeft", 0f },
                { "MarginTop", 0f },
                { "MarginRight", 0f },
                { "MarginBottom", 0f },
                { "PaddingLeft", 0f },
                { "PaddingTop", 0f },
                { "PaddingRight", 0f },
                { "PaddingBottom", 0f },
                { "MinWidth", 0f },
                { "MinWidthOfParent", 0f },
                { "MinWidthType", MinMaxSizeType.Fixed },
                { "MaxWidth", 100f },
                { "MaxWidthOfParent", 1f },
                { "MaxWidthType", MinMaxSizeType.None },
                { "MinHeight", 0f },
                { "MinHeightOfParent", 0f },
                { "MinHeightType", MinMaxSizeType.Fixed },
                { "MaxHeight", 100f },
                { "MaxHeightOfParent", 1f },
                { "MaxHeightType", MinMaxSizeType.None },
                { "SkipLayout", false }
            } },
            { typeof(FlexalonFlexibleLayout), new Dictionary<string, object> {
                { "Direction", Direction.PositiveX },
                { "Wrap", false },
                { "WrapDirection", Direction.NegativeY },
                { "HorizontalAlign", Align.Start },
                { "VerticalAlign", Align.End },
                { "HorizontalInnerAlign", Align.Start },
                { "VerticalInnerAlign", Align.End },
                { "GapType", FlexalonFlexibleLayout.GapOptions.Fixed },
                { "Gap", 0f },
                { "WrapGapType", FlexalonFlexibleLayout.GapOptions.Fixed },
                { "WrapGap", 0f },
            } },
            { typeof(FlexalonGridLayout), new Dictionary<string, object> {
                { "Columns", 1u },
                { "Rows", 1u },
                { "ColumnDirection", Direction.PositiveX },
                { "RowDirection", Direction.NegativeY },
                { "ColumnSpacing", 0f },
                { "RowSpacing", 0f },
                { "HorizontalAlign", Align.Start },
                { "VerticalAlign", Align.Start },
            } },
            { typeof(TextMeshProUGUI), new Dictionary<string, object> {
                { "text", "" },
                { "font", AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath("8f586378b4e144a9851e7b34d9b748ee")) }, // Liberation Sans SDF
                { "fontSize", 16f },
                { "fontStyle", FontStyles.Normal },
                { "color", Color.black },
                { "alignment", TextAlignmentOptions.TopLeft },
                { "overflowMode", TextOverflowModes.Overflow },
                { "enableWordWrapping", true },
            } },
            { typeof(TMP_InputField), new Dictionary<string, object> {
                { "text", "" },
                { "targetGraphic", null },
                { "colors", ColorBlock.defaultColorBlock },
                { "textViewport", null },
                { "textComponent", null },
                { "fontAsset", AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath("8f586378b4e144a9851e7b34d9b748ee")) }, // Liberation Sans SDF
                { "pointSize", 16f },
                { "characterLimit", 0 },
                { "contentType", TMP_InputField.ContentType.Standard },
                { "lineType", TMP_InputField.LineType.SingleLine },
                { "placeholder", null },
                { "verticalScrollbar", null },
                { "readOnly", false },
                { "interactable", true },
            } },
            { typeof(Button), new Dictionary<string, object> {
                { "interactable", true },

            } },
            { typeof(Image), new Dictionary<string, object> {
                { "sprite", null },
                { "color", new Color(0, 0, 0, 0) },
                { "type", Image.Type.Sliced },
                { "preserveAspect", false },
            } },
            { typeof(Toggle), new Dictionary<string, object> {
                { "targetGraphic", null },
                { "colors", ColorBlock.defaultColorBlock },
                { "isOn", false },
                { "graphic", null },
                { "interactable", true },
            } },
            { typeof(Text), new Dictionary<string, object> {
                { "text", "" }
            } },
            { typeof(TMP_Dropdown), new Dictionary<string, object> {
                { "interactable", true },
                { "options", new List<TMP_Dropdown.OptionData>() }
            } },
            { typeof(Slider), new Dictionary<string, object> {
                { "interactable", true },
                { "minValue", 0 },
                { "maxValue", 100 },
                { "value", 50 },
                { "fillRect", null }
            } },
            { typeof(ScrollRect), new Dictionary<string, object> {
                { "content", null },
                { "viewport", null },
                { "horizontalScrollbar", null },
                { "verticalScrollbar", null },
                { "horizontal", false },
                { "vertical", false },
                { "horizontalScrollbarVisibility", ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport },
                { "verticalScrollbarVisibility", ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport },
            } },
            { typeof(Mask), new Dictionary<string, object> {
            } },
            { typeof(RectMask2D), new Dictionary<string, object> {
            } },
            { typeof(Scrollbar), new Dictionary<string, object> {
            } },
            { typeof(Canvas), new Dictionary<string, object> {
            } },
            { typeof(FlexalonStyleComponent), new Dictionary<string, object> {
                { "Style", null },
            } },
            { typeof(FlexalonImage), new Dictionary<string, object> {
            } },
        };

        public static readonly Dictionary<string, Type> ComponentTypes = SerializedTypes.Keys.ToDictionary(x => x.Name, x => x);

        public static void ApplyDefaults(Component component)
        {
            var defaults = SerializedTypes[component.GetType()];
            foreach (var kvp in defaults)
            {
                PropertyOrField.TrySet(component, kvp.Key, kvp.Value);
            }
        }

        public static void ApplyDefault(Component component, string property)
        {
            if (TryGetDefault<object>(component.GetType(), property, out var value))
            {
                PropertyOrField.TrySet(component, property, value);
            }
        }

        public static bool TryGetDefault<T>(Type componentType, string property, out T defaultValue)
        {
            if (SerializedTypes.TryGetValue(componentType, out var serializedType))
            {
                if (serializedType.TryGetValue(property, out var value))
                {
                    defaultValue = (T)value;
                    return true;
                }
            }

            defaultValue = default(T);
            return false;
        }

        public static T GetDefault<T>(string componentType, string property)
        {
            if (ComponentTypes.TryGetValue(componentType, out var type))
            {
                return GetDefault<T>(type, property);
            }

            return default(T);
        }

        public static T GetDefault<T>(Type componentType, string property)
        {
            if (TryGetDefault<T>(componentType, property, out var defaultValue))
            {
                return defaultValue;
            }

            return default(T);
        }
    }
}

#endif