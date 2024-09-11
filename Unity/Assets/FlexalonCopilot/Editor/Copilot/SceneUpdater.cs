#if UNITY_TMPRO && UNITY_UI

using System.Threading.Tasks;
using Flexalon;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace FlexalonCopilot.Editor
{
    internal class SceneUpdater
    {
        private GameObjectIdMap _gids;
        private IdMap<Component> _cids = new IdMap<Component>();
        private UpdateLog _log;
        private GameObject _root;
        private PrefabSet _prefabSet;

        public GameObjectIdMap Gids => _gids;

        public string PlaceholderSpriteGuid;

        public SceneUpdater(GameObject root, PrefabSet prefabSet, UpdateLog log, GameObjectIdMap gids)
        {
            _root = root;
            _prefabSet = prefabSet;
            _log = log;
            _gids = gids;
        }

        public void Init()
        {
            AnimatedPropertyOrField.RecordEdits = true;
        }

        public void EnableAnimations()
        {
            AnimatedPropertyOrField.EnableAnimations = true;
        }

        public void CreateGameObject(string id, string name, string prefab)
        {
            var parts = id.Split('.');
            if (parts.Length == 3)
            {
                LinkReferencedGameObject(id, parts);
                return;
            }

            GameObject gameObject = null;

            if (!string.IsNullOrEmpty(prefab))
            {
                gameObject = CreatePrefab(name, prefab);
            }
            else
            {
                _log.Log($"Create GameObject {name}");
                gameObject = new GameObject(name);
            }

            Undo.RegisterCreatedObjectUndo(gameObject, "Create GameObject");
            UndoSetTransformParent(gameObject.transform, _root.transform);

            _gids.Add(id, gameObject);
        }

        private void UndoSetTransformParent(Transform child, Transform parent)
        {
            var pos = child.localPosition;
            var rot = child.localRotation;
            var scale = child.localScale;
            Undo.SetTransformParent(child, parent, "Set Parent");
            child.localPosition = pos;
            child.localRotation = rot;
            child.localScale = scale;
        }

        private void UndoRegisterChildrenOrderUndo(Transform transform)
        {
            var method = typeof(Undo).GetMethod("RegisterChildrenOrderUndo", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            if (method != null)
            {
                method.Invoke(null, new object[] { transform, "Reorder Children" });
            }
        }

        private GameObject CreatePrefab(string name, string prefab)
        {
            GameObject prefabGameObject = null;

            switch (prefab)
            {
                case "Flexalon:Panel":
                    prefabGameObject = _prefabSet.PanelPrefab.gameObject;
                    break;
                case "Flexalon:Image":
                    prefabGameObject = _prefabSet.ImagePrefab.gameObject;
                    break;
                case "Flexalon:Button":
                    prefabGameObject = _prefabSet.ButtonPrefab.gameObject;
                    break;
                case "Flexalon:Toggle":
                    prefabGameObject = _prefabSet.TogglePrefab.gameObject;
                    break;
                case "Flexalon:Slider":
                    prefabGameObject = _prefabSet.SliderPrefab.gameObject;
                    break;
                case "Flexalon:HorizontalScrollbar":
                    prefabGameObject = _prefabSet.HorizontalScrollbarPrefab.gameObject;
                    break;
                case "Flexalon:VerticalScrollbar":
                    prefabGameObject = _prefabSet.VerticalScrollbarPrefab.gameObject;
                    break;
                case "Flexalon:ScrollRect":
                    prefabGameObject = _prefabSet.ScrollRectPrefab.gameObject;
                    break;
                case "Flexalon:InputField":
                    prefabGameObject = _prefabSet.InputFieldPrefab.gameObject;
                    break;
                case "Flexalon:Dropdown":
                    prefabGameObject = _prefabSet.DropdownPrefab.gameObject;
                    break;
            }

            if (prefabGameObject == null)
            {
                Log.Error("Could not find prefab: " + prefab);
                return new GameObject();
            }

            _log.Log($"Create GameObject {name} from prefab {prefabGameObject.name}");

            var gameObject = GameObject.Instantiate(prefabGameObject);
            gameObject.name = name;
            return gameObject;
        }

        private void LinkReferencedGameObject(string id, string[] parts)
        {
            if (!_gids.TryGetGameObject(parts[0], out var refGo))
            {
                Log.Error($"LinkReferencedGameObject: GameObject {parts[0]} not found.");
                return;
            }

            var component = refGo.GetComponent(parts[1]);
            if (component == null)
            {
                Log.Error($"LinkReferencedGameObject: Component {parts[1]} not found on {refGo.name}.");
                return;
            }

            if (!PropertyOrField.TryGet<UnityEngine.Object>(component, parts[2], out var value))
            {
                Log.Error($"LinkReferencedGameObject: Invalid reference {parts[2]} on component {component.GetType().Name} of {refGo.name}.");
                return;
            }

            if (value is GameObject go)
            {
                Log.Verbose($"Link {id} => {go.name}");
                _gids.Add(id, go);
                return;
            }

            if (value is Component targetComponent)
            {
                Log.Verbose($"Link {id} => {targetComponent.name}");
                _gids.Add(id, targetComponent.gameObject);
                return;
            }

            Log.Error($"LinkReferencedGameObject: Invalid reference {parts[2]} on component {component.GetType().Name} of {refGo.name}.");
        }

        public void MoveGameObject(string id, string parentId, string indexStr)
        {
            if (!_gids.TryGetGameObject(id, out var gameObject))
            {
                Log.Warning($"Move GameObject {id} not found");
                return;
            }

            if (!_gids.TryGetGameObject(parentId, out var parent))
            {
                Log.Warning($"Move GameObject {id} parent {parentId} not found");
                return;
            }

            if (!int.TryParse(indexStr, out var index))
            {
                Log.Warning($"Move GameObject {id} invalid index {indexStr}");
                return;
            }

            bool sameParent = gameObject.transform.parent == parent.transform;
            if (index < 0)
            {
                index = parent.transform.childCount;
                if (sameParent)
                {
                    index--;
                }
            }

            if (sameParent && index == gameObject.transform.GetSiblingIndex())
            {
                Log.Verbose($"Move GameObject {gameObject.name} to {parent.name} at index {index} (no-op)");
                return;
            }

            _log.Log($"Move GameObject {gameObject.name} to {parent.name} at index {index}");
            Undo.RecordObject(gameObject.transform, "Move GameObject");
            UndoSetTransformParent(gameObject.transform, parent.transform);
            UndoRegisterChildrenOrderUndo(parent.transform);

            gameObject.transform.SetSiblingIndex(index);

            if (gameObject.transform is RectTransform rectTransform)
            {
                rectTransform.anchoredPosition = Vector2.zero;
            }
            else
            {
                gameObject.transform.localPosition = Vector3.zero;
            }
        }

        public void DestroyGameObject(string id)
        {
            if (_gids.TryGetGameObject(id, out var gameObject))
            {
                if (gameObject == _root && _root.GetComponent<Canvas>() != null)
                {
                    while (_root.transform.childCount > 0)
                    {
                        DestroyGameObject(_root.transform.GetChild(0).gameObject);
                    }
                }
                else
                {
                    DestroyGameObject(gameObject);
                }
            }
        }

        private void DestroyGameObject(GameObject go)
        {
            _log.Log($"Destroy GameObject {go.name}");

            foreach (var component in go.GetComponents<Component>())
            {
                _cids.Remove(component);
            }

            _gids.Remove(go);

            Undo.DestroyObjectImmediate(go);
        }

        public void AddComponent(string id, string cid, string type)
        {
            if (_gids.TryGetGameObject(id, out var gameObject) && SerializedComponents.ComponentTypes.TryGetValue(type, out var componentType))
            {
                var component = gameObject.GetComponent(componentType);
                if (component == null)
                {
                    bool initFlexalonObject = componentType.IsSubclassOf(typeof(Flexalon.LayoutBase)) && !gameObject.GetComponent<FlexalonObject>();
                    bool initRectTransform = componentType != typeof(RectTransform) && !(gameObject.transform is RectTransform);

                    _log.Log($"Add Component {type} to {gameObject.name}");
                    component = Flexalon.Flexalon.AddComponent(gameObject, componentType);

                    if (component == null)
                    {
                        Log.Error($"Failed to add component {type} to {gameObject.name}");
                        return;
                    }

                    InitializeComponent(component);

                    // When we instantiate a layout, we might also implicitly instantiate a FlexalonObject, which needs initialization.
                    if (initFlexalonObject)
                    {
                        var obj = component.GetComponent<FlexalonObject>();
                        SerializedComponents.ApplyDefaults(obj);
                        if (!obj.GetComponent<Canvas>())
                        {
                            obj.WidthType = SizeType.Layout;
                            obj.HeightType = SizeType.Layout;
                            obj.DepthType = SizeType.Layout;
                        }
                    }

                    // When we instantiate a component that requires a RectTransform, we need to initialize it.
                    if (initRectTransform && gameObject.transform is RectTransform rectTransform)
                    {
                        SerializedComponents.ApplyDefaults(rectTransform);
                    }
                }
                else
                {
                    Log.Verbose($"Component {type} already exists on {gameObject.name}, mapping: {cid} => {component.GetInstanceID()}");
                }

                _cids.Add(cid, component);
            }
        }

        private void InitializeComponent(Component component)
        {
            SerializedComponents.ApplyDefaults(component);

            if (component is Mask mask)
            {
                mask.showMaskGraphic = false;
            }
        }

        public void RemoveComponent(string cid)
        {
            if (_cids.TryGetObject(cid, out var component))
            {
                _log.Log($"Remove Component {component.GetType().Name} from {component.name}");
                _cids.Remove(component);
                Undo.DestroyObjectImmediate(component);
            }
        }

        public void SetComponentProperty(string cid, string property, string serializedValue)
        {
            if (_cids.TryGetObject(cid, out var component))
            {
                var prop = new AnimatedPropertyOrField(component.GetType(), property);
                if (prop.Type.IsSubclassOf(typeof(Component)) && _cids.TryGetObject(serializedValue, out var refComponent))
                {
                    Log.Verbose($"Resolving component reference {serializedValue} => {refComponent.GetInstanceID()}");
                    serializedValue = refComponent.GetInstanceID().ToString();
                }
                else if (prop.Type == typeof(GameObject) && _gids.TryGetGameObject(serializedValue, out var refGo))
                {
                    Log.Verbose($"Resolving gameObject reference {serializedValue} => {refGo.GetInstanceID()}");
                    serializedValue = refGo.GetInstanceID().ToString();
                }

                if (!Serialization.TryDeserialize(prop.Type, serializedValue, out var value))
                {
                    if (prop.Type == typeof(Sprite) && PlaceholderSpriteGuid != null)
                    {
                        Log.Warning($"Failed to deserialize {serializedValue} to {prop.Type}, using placeholder sprite");
                        Serialization.TryDeserialize(prop.Type, PlaceholderSpriteGuid, out value);
                    }
                    else
                    {
                        Log.Warning($"Failed to deserialize {serializedValue} to {prop.Type}");
                        return;
                    }
                }

                if (FlexalonStyle.IsInherited(component.GetType(), property) || FlexalonStyle.IsPropertySetByStyle(component, property, false))
                {
                    if (SetStyleProperty(component.gameObject, component.GetType().Name, property, serializedValue))
                    {
                        _log.Log($"Set {component.name} > FlexalonStyle > {component.GetType().Name} > {property} > {serializedValue}");
                    }
                }
                else
                {
                    if (prop.SetValue(component, value))
                    {
                        _log.Log($"Set {component.name} > {component.GetType().Name} > {property} > {serializedValue}");
                    }
                }
            }
            else
            {
                Log.Warning($"Failed to find component {cid}");
            }
        }

        public void SetStyleProperty(string id, string component, string property, string serializedValue)
        {
            if (_gids.TryGetGameObject(id, out var gameObject))
            {
                if (SetStyleProperty(gameObject, component, property, serializedValue))
                {
                    _log.Log($"Set {gameObject.name} > FlexalonStyle > {component} > {property} > {serializedValue}");
                }
            }
        }

        public bool SetStyleProperty(GameObject gameObject, string component, string property, string serializedValue)
        {
            var inlineStyle = FlexalonStyle.GetOrCreateInlineStyle(gameObject, component, property);
            Undo.RecordObject(inlineStyle.Style, "Set Style Property");
            if (inlineStyle.Style.SetProperty(component, property, serializedValue))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(inlineStyle);
                return true;
            }

            return false;
        }

        public void ClearComponentProperty(string cid, string property)
        {
            if (_cids.TryGetObject(cid, out var component))
            {
                Undo.RecordObject(component, "Clear Component Property");
                _log.Log($"Clear {component.name} > {component.GetType().Name} > {property}");
                SerializedComponents.ApplyDefault(component, property);
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                ClearStyleProperty(component.gameObject, component.GetType().Name, property);
            }
        }

        public void ClearStyleProperty(string id, string component, string property)
        {
            if (_gids.TryGetGameObject(id, out var gameObject))
            {
                ClearStyleProperty(gameObject, component, property);
            }
        }

        public void ClearStyleProperty(GameObject gameObject, string component, string property)
        {
            var inlineStyle = FlexalonStyle.GetInlineStyle(gameObject, component, property);
            if (inlineStyle != null)
            {
                Undo.RecordObject(inlineStyle.Style, "Clear Style Property");
                if (inlineStyle.Style.ClearProperty(component, property))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(inlineStyle);
                    _log.Log($"Clear {gameObject.name} > FlexalonStyle > {component} > {property}");
                }
            }
        }

        public void SetRectSizeToZero(string id, bool width)
        {
            if (!_gids.TryGetGameObject(id, out var gameObject))
            {
                return;
            }

            if (gameObject.transform is RectTransform rectTransform)
            {
                Undo.RecordObject(rectTransform, "Set Rect Size To Zero");

                if (width)
                {
                    if (rectTransform.rect.width != 0)
                    {
                        _log.Log($"Set {gameObject.name}.RectTransform.width > 0");
                        Undo.RecordObject(rectTransform, "Set Rect Width To Zero");
                        PrefabUtility.RecordPrefabInstancePropertyModifications(rectTransform);
                        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
                    }
                }
                else
                {
                    if (rectTransform.rect.height != 0)
                    {
                        _log.Log($"Set {gameObject.name}.RectTransform.height > 0");
                        Undo.RecordObject(rectTransform, "Set Rect Height To Zero");
                        PrefabUtility.RecordPrefabInstancePropertyModifications(rectTransform);
                        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
                    }
                }
            }
        }

        public void PostCommand()
        {
            // TODO: Pretty inefficient
            FlexalonStyle.ApplyAllStyles(_root);
        }

        public async Task PostUpdate()
        {
            await AnimationUpdater.Instance.WaitForAll();
            AnimatedPropertyOrField.EnableAnimations = false;
            AnimatedPropertyOrField.RecordEdits = false;
        }

        private string _lastMessage;

        public void Message(string type, string message)
        {
            // Avoid repeating yourself.
            if (message == _lastMessage)
            {
                return;
            }

            var color = type == "1" ? "yellow" : "white";
            var logMessage = $"\n<color={color}>" + message + "</color>\n";
            _log.Log(logMessage);
            _lastMessage = message;
        }
    }
}

#endif