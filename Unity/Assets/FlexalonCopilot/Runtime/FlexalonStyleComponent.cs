#if UNITY_TMPRO && UNITY_UI

using UnityEngine;

namespace FlexalonCopilot
{
    [ExecuteAlways, AddComponentMenu("Flexalon/Flexalon Style")]
    public class FlexalonStyleComponent : MonoBehaviour
    {
        [SerializeField]
        private FlexalonStyle _style;
        public FlexalonStyle Style
        {
            get => _style;
            set
            {
                _style = value;
                FlexalonStyle.ApplyAllStyles(gameObject);
            }
        }
    }
}

#endif