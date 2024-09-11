using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlexalonCopilot.Editor
{
    [Serializable]
    internal class ChatLogEntry
    {
        public string Id;
        public string Prompt;
        public string Response;
    }

    [CreateAssetMenu(fileName = "ChatLog", menuName = "Flexalon Copilot/Chat Log", order = 281)]
    internal class ChatLog : ScriptableObject
    {
        public List<ChatLogEntry> Entries = new List<ChatLogEntry>();
    }
}