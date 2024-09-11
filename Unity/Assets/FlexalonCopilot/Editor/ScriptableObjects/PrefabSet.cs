#if UNITY_TMPRO && UNITY_UI

using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace FlexalonCopilot
{
    [CreateAssetMenu(fileName = "Prefab Set", menuName = "Flexalon Copilot/Prefab Set", order = 281)]
    public class PrefabSet : ScriptableObject
    {
        [SerializeField]
        private FlexalonPanel _panelPrefab = null;
        public FlexalonPanel PanelPrefab
        {
            get => _panelPrefab;
            set => PanelPrefab = value;
        }

        [SerializeField]
        private Image _imagePrefab = null;
        public Image ImagePrefab
        {
            get => _imagePrefab;
            set => _imagePrefab = value;
        }

        [SerializeField]
        private Button _buttonPrefab = null;
        public Button ButtonPrefab
        {
            get => _buttonPrefab;
            set => _buttonPrefab = value;
        }

        [SerializeField]
        private Toggle _togglePrefab = null;
        public Toggle TogglePrefab
        {
            get => _togglePrefab;
            set => _togglePrefab = value;
        }

        [SerializeField]
        private Slider _sliderPrefab = null;
        public Slider SliderPrefab
        {
            get => _sliderPrefab;
            set => _sliderPrefab = value;
        }

        [SerializeField]
        private Scrollbar _horizontalSrollbarPrefab = null;
        public Scrollbar HorizontalScrollbarPrefab
        {
            get => _horizontalSrollbarPrefab;
            set => _horizontalSrollbarPrefab = value;
        }

        [SerializeField]
        private Scrollbar _verticalSrollbarPrefab = null;
        public Scrollbar VerticalScrollbarPrefab
        {
            get => _verticalSrollbarPrefab;
            set => _verticalSrollbarPrefab = value;
        }

        [SerializeField]
        private ScrollRect _scrollRectPrefab = null;
        public ScrollRect ScrollRectPrefab
        {
            get => _scrollRectPrefab;
            set => _scrollRectPrefab = value;
        }

        [SerializeField]
        private TMP_InputField _inputFieldPrefab = null;
        public TMP_InputField InputFieldPrefab
        {
            get => _inputFieldPrefab;
            set => _inputFieldPrefab = value;
        }

        [SerializeField]
        private TMP_Dropdown _dropdownPrefab = null;
        public TMP_Dropdown DropdownPrefab
        {
            get => _dropdownPrefab;
            set => _dropdownPrefab = value;
        }

        // Default to "LightPrefabSet" if it exists, otherwise the default prefab set.
        public static PrefabSet Default => AssetDatabase.LoadAssetAtPath<PrefabSet>(AssetDatabase.GUIDToAssetPath("1190172d82072174f869c8cd8acacfcb")) ??
            AssetDatabase.LoadAssetAtPath<PrefabSet>(AssetDatabase.GUIDToAssetPath("79184dfc7ffe805409bb3b4633e3f3a6"));

        internal static PrefabSet Test =>
            AssetDatabase.LoadAssetAtPath<PrefabSet>(AssetDatabase.GUIDToAssetPath("79184dfc7ffe805409bb3b4633e3f3a6"));
    }
}

#endif