using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FlexalonCopilot.Editor
{
    internal class IdMap<T> where T : UnityEngine.Object
    {
        private Dictionary<string, T> _idToObject = new Dictionary<string, T>();
        private Dictionary<T, string> _objectToId = new Dictionary<T, string>();

        public IdMap()
        {
        }

        public IdMap(string serialized)
        {
            Deserialize(serialized);
        }

        public void Add(string id, T obj)
        {
            _idToObject[id] = obj;
            _objectToId[obj] = id;
        }

        public void Remove(string id)
        {
            if (_idToObject.TryGetValue(id, out var obj))
            {
                _objectToId.Remove(obj);
                _idToObject.Remove(id);
            }
        }

        public void Remove(T obj)
        {
            if (_objectToId.TryGetValue(obj, out var id))
            {
                _idToObject.Remove(id);
                _objectToId.Remove(obj);
            }
        }

        public bool TryGetObject(string id, out T obj)
        {
            if (_idToObject.TryGetValue(id, out obj))
            {
                return true;
            }

            if (int.TryParse(id, out var iid))
            {
                obj = EditorUtility.InstanceIDToObject(iid) as T;
                if (obj != null)
                {
                    return true;
                }
            }

            return false;
        }

        public string GetId(T obj)
        {
            if (_objectToId.TryGetValue(obj, out var id))
            {
                return id;
            }

            return obj.GetInstanceID().ToString();
        }

        public string Serialize()
        {
            return string.Join("\n", _idToObject.Select(kvp => kvp.Key + " " + kvp.Value.GetInstanceID()));
        }

        public void Deserialize(string serialized)
        {
            _idToObject.Clear();
            _objectToId.Clear();

            if (string.IsNullOrWhiteSpace(serialized))
            {
                return;
            }

            var lines = serialized.Split('\n');
            foreach (var line in lines)
            {
                var lastSpace = line.LastIndexOf(' ');
                if (lastSpace == -1)
                {
                    Log.Warning("Invalid line: " + line);
                    continue;
                }

                if (int.TryParse(line.Substring(lastSpace + 1), out var iidParsed))
                {
                    var obj = EditorUtility.InstanceIDToObject(iidParsed) as T;
                    if (obj != null)
                    {
                        var id = line.Substring(0, lastSpace);
                        Add(id, obj);
                    }
                    else
                    {
                        Log.Warning("Could not find object with id: " + iidParsed);
                    }
                }
                else
                {
                    Log.Warning("Could not parse id: " + line.Substring(lastSpace + 1));
                }
            }
        }
    }

    internal class GameObjectIdMap
    {
        private IdMap<GameObject> _idMap = new IdMap<GameObject>();

        public void Add(string id, GameObject go)
        {
            var data = go.GetComponent<FlexalonCopilotData>();
            if (data == null)
            {
                data = Flexalon.Flexalon.AddComponent<FlexalonCopilotData>(go);
            }

            data.GeneratedId = id;
            _idMap.Add(id, go);
        }

        public string GetId(GameObject go)
        {
            if (go.TryGetComponent<FlexalonCopilotData>(out var data) && !string.IsNullOrWhiteSpace(data.GeneratedId))
            {
                _idMap.Add(data.GeneratedId, go);
                return data.GeneratedId;
            }

            return _idMap.GetId(go);
        }

        public bool TryGetGameObject(string id, out GameObject go)
        {
            return _idMap.TryGetObject(id, out go);
        }

        public void Remove(GameObject gameObject)
        {
            if (gameObject.TryGetComponent<FlexalonCopilotData>(out var data))
            {
                data.GeneratedId = null;
            }

            _idMap.Remove(gameObject);
        }
    }
}