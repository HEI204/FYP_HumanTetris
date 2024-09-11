using System.Collections.Generic;

namespace FlexalonCopilot.Editor
{
    internal class UpdateLog
    {
        private List<string> _logs = new List<string>();
        public IReadOnlyList<string> Logs => _logs;

        public void Log(string message)
        {
            FlexalonCopilot.Log.Verbose(message);
            _logs.Add(message);
        }

        public string TakeLogs()
        {
            var message = string.Join("\n", _logs);
            _logs.Clear();
            return message;
        }
    }
}