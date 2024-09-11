using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FlexalonCopilot
{
    [ExecuteAlways]
    internal class AnimationUpdater : MonoBehaviour
    {
        private static AnimationUpdater _instance;
        public static AnimationUpdater Instance
        {
            get
            {
                if (_instance == null)
                {
                    #if UNITY_2023_1_OR_NEWER
                        _instance = FindFirstObjectByType<AnimationUpdater>();
                    #else
                        _instance = FindObjectOfType<AnimationUpdater>();
                    #endif
                }

                if (_instance == null)
                {
                    var go = new GameObject("AnimationUpdater");
                    _instance = go.AddComponent<AnimationUpdater>();
                    go.hideFlags = HideFlags.HideAndDontSave;
                }

                return _instance;
            }
        }

        private Dictionary<(object, string), AnimatedPropertyOrField> _props = new Dictionary<(object, string), AnimatedPropertyOrField>();
        private List<(object, string)> _done = new List<(object, string)>();

        public void Register(AnimatedPropertyOrField prop)
        {
            _props[(prop.Obj, prop.Name)] = prop;
        }

        public void Remove(object obj, string name)
        {
            _props.Remove((obj, name));
        }

        public void ForceFinish(object obj, string name)
        {
            if (_props.TryGetValue((obj, name), out var prop))
            {
                prop.Finish();
                _props.Remove((obj, name));
            }
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                Tick(Time.smoothDeltaTime);
            }
        }

        private void Tick(float deltaTime)
        {
#if UNITY_EDITOR
            if (_props.Count > 0)
            {
                Flexalon.Flexalon.RecordFrameChanges = true;
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            }
#endif

            foreach (var kvp in _props)
            {
                kvp.Value.Update(deltaTime);
                if (kvp.Value.Done)
                {
                    _done.Add(kvp.Key);
                }
            }

            foreach (var key in _done)
            {
                _props.Remove(key);
            }

            _done.Clear();
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            UnityEditor.EditorApplication.update += EditorUpdate;
        }

        private void OnDisable()
        {
            UnityEditor.EditorApplication.update -= EditorUpdate;
        }

        private DateTime _lastTime = DateTime.Now;

        private void EditorUpdate()
        {
            var now = DateTime.Now;
            var deltaTime = (float)(now - _lastTime).TotalSeconds;
            if (deltaTime >= 1/30f)
            {
                _lastTime = now;
                Tick(deltaTime);
            }
        }
#endif

        public async Task WaitForAll()
        {
            int max = 10;
            while ((Flexalon.Flexalon.RecordFrameChanges || _props.Count > 0) && max-- > 0)
            {
                await Task.Delay(100);
            }

            foreach (var kvp in _props)
            {
                kvp.Value.Finish();
            }

            _props.Clear();
        }
    }
}