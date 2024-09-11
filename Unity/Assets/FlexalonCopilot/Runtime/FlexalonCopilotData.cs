using UnityEngine;

namespace FlexalonCopilot
{
    [ExecuteAlways]
    internal class FlexalonCopilotData : MonoBehaviour
    {
        public string GeneratedId;

        public static string GetId(GameObject go)
        {
            if (go.TryGetComponent<FlexalonCopilotData>(out var data))
            {
                return data.GeneratedId;
            }

            return go.GetInstanceID().ToString();
        }

        void Awake()
        {
            hideFlags = HideFlags.HideInInspector;
        }
    }
}