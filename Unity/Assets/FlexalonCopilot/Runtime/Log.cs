using System.Diagnostics;

namespace FlexalonCopilot
{
    internal class Log
    {
        [Conditional("FLEXALON_COPILOT_LOG_VERBOSE")]
        public static void Verbose(string message)
        {
            UnityEngine.Debug.Log("FCV: " + message);
        }

        public static void Info(string message)
        {
            UnityEngine.Debug.Log("FC: " + message);
        }

        public static void Warning(string message)
        {
            UnityEngine.Debug.LogWarning("FC: " + message);
        }

        public static void Error(string message)
        {
            UnityEngine.Debug.LogError("FC: " + message);
        }

        public static void Exception(System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }
}