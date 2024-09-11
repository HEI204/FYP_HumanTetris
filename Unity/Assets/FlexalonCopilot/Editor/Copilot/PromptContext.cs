#if UNITY_TMPRO && UNITY_UI

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace FlexalonCopilot.Editor
{
    [Serializable]
    internal class PromptContext
    {
        public List<SerializedGameObject> gameObjects;
        public List<string> selectedGameObjects;
        public List<SerializedAsset> assets;
        public List<string> selectedAssets;
    }

    [Serializable]
    internal class SerializedGameObject
    {
        public string id;
        public string name;
        public string parentId;
        public int siblingIndex;
        public List<SerializedComponent> components;
        public List<FlexalonStyleProperty> styleProperties;
    }

    [Serializable]
    internal class SerializedComponent
    {
        public int id;
        public string type;
        public List<SerializedComponentProperty> properties;
    }

    [Serializable]
    internal class SerializedAsset
    {
        public string id;
        public string type;
        public string name;
    }

    [Serializable]
    internal class SerializedStyle : SerializedAsset
    {
        public List<FlexalonStyleProperty> properties;
    }

    [Serializable]
    internal class SerializedComponentProperty
    {
        public string name;
        public string value;
    }

    internal class PromptContextFactory
    {
        public static PromptContext Create(GameObject root, List<UnityEngine.Object> contextObjects, GameObjectIdMap gids)
        {
            var contextAssets = contextObjects.Where(o => AssetDatabase.Contains(o));
            var contextAssetGuids = contextAssets.Select(o => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o)));
            var contextGameObjects = contextObjects.Except(contextAssets).OfType<GameObject>();

            var context = new PromptContext();
            context.gameObjects = SerializeGameObjects(root, gids);

            var selectedGameObjects = Selection.gameObjects.Union(contextGameObjects).Where(go => go != root && go.transform.IsChildOf(root.transform));
            context.selectedGameObjects = selectedGameObjects.Select(go => gids.GetId(go)).ToList();

            var selectedAssets = Selection.assetGUIDs.Union(contextAssetGuids)
                .ToDictionary(guid => guid, guid => GetAsset(guid))
                .Where(kvp => IsSupportedAsset(kvp.Value))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            context.selectedAssets = selectedAssets.Keys.ToList();

            var allAssets = selectedAssets;
            FindReferencedAssets(context, allAssets);
            context.assets = SerializeAssets(allAssets);
            return context;
        }

        private static List<SerializedGameObject> SerializeGameObjects(GameObject root, GameObjectIdMap gids)
        {
            var gameObjects = new List<SerializedGameObject>();
            var queue = new Queue<GameObject>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var gameObject = queue.Dequeue();
                Log.Verbose($"Serializing game object {gameObject.name}");

                var serializedGameObject = new SerializedGameObject();
                serializedGameObject.id = gids.GetId(gameObject);
                if (gameObject.transform.parent)
                {
                    serializedGameObject.parentId = gids.GetId(gameObject.transform.parent.gameObject);
                }

                serializedGameObject.siblingIndex = gameObject.transform.GetSiblingIndex();
                serializedGameObject.name = gameObject.name;
                serializedGameObject.styleProperties = FlexalonStyle.GetInlineStyleProperties(gameObject);
                gameObjects.Add(serializedGameObject);

                serializedGameObject.components = new List<SerializedComponent>();
                foreach (var component in gameObject.GetComponents<Component>())
                {
                    if (!SerializedComponents.SerializedTypes.TryGetValue(component.GetType(), out var serializedType))
                    {
                        continue;
                    }

                    Log.Verbose($"Serializing component {component.GetType().Name}");

                    var serializedComponent = new SerializedComponent();
                    serializedComponent.id = component.GetInstanceID();
                    serializedComponent.type = component.GetType().Name;
                    serializedComponent.properties = new List<SerializedComponentProperty>();

                    serializedGameObject.components.Add(serializedComponent);

                    foreach (var kvp in serializedType)
                    {
                        var property = kvp.Key;
                        var defaultValue = kvp.Value;

                        // Skip inline style properties
                        if (serializedGameObject.styleProperties.Any(p => p.Component == serializedComponent.type && p.Property == property))
                        {
                            continue;
                        }

                        // Don't serialize properties overridden by a style.
                        if (FlexalonStyle.IsPropertySetByStyle(component, property, true))
                        {
                            Log.Verbose($"Serializing skip property {serializedComponent.type}.{property} because it's overridden by a style.");
                            continue;
                        }

                        // If there's an inline style, prefer that over the component value.
                        var prop = new PropertyOrField(component.GetType(), property);
                        var value = prop.GetValue(component);

                        if (value == null && defaultValue == null || (value != null && !value.Equals(defaultValue)))
                        {
                            var serializedValue = Serialization.Serialize(value);
                            if (FlexalonStyle.IsInherited(serializedComponent.type, property))
                            {
                                Log.Verbose($"Serializing inline style property {serializedComponent.type}.{property} = {Serialization.Serialize(value)} ({prop.Type.Name})");
                                serializedGameObject.styleProperties.Add(new FlexalonStyleProperty
                                {
                                    Component = serializedComponent.type,
                                    Property = property,
                                    Value = serializedValue
                                });
                            }
                            else
                            {
                                Log.Verbose($"Serializing property {serializedComponent.type}.{property} = {Serialization.Serialize(value)} ({prop.Type.Name})");
                                serializedComponent.properties.Add(new SerializedComponentProperty
                                {
                                    name = property,
                                    value = serializedValue
                                });
                            }
                        }
                    }
                }

                if (gameObject.GetComponent<TMP_Dropdown>() == null)
                {
                    foreach (Transform child in gameObject.transform)
                    {
                        queue.Enqueue(child.gameObject);
                    }
                }
            }

            return gameObjects;
        }

        private static UnityEngine.Object GetAsset(string guid)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        }

        private static void FindReferencedAssets(PromptContext context, Dictionary<string, UnityEngine.Object> guids)
        {
            foreach (var gameObject in context.gameObjects)
            {
                foreach (var component in gameObject.components)
                {
                    foreach (var property in component.properties)
                    {
                        if (property.value?.Length == 32)
                        {
                            var asset = GetAsset(property.value);
                            if (IsSupportedAsset(asset))
                            {
                                guids[property.value] = asset;
                            }
                        }
                    }
                }

                foreach (var prop in gameObject.styleProperties)
                {
                    if (prop.Value.Length == 32)
                    {
                        var asset = GetAsset(prop.Value);
                        if (IsSupportedAsset(asset))
                        {
                            guids[prop.Value] = asset;
                        }
                    }
                }
            }
        }

        private static List<SerializedAsset> SerializeAssets(Dictionary<string, UnityEngine.Object> assets)
        {
            var result = new List<SerializedAsset>();
            foreach (var kvp in assets)
            {
                var guid = kvp.Key;
                var asset = kvp.Value;

                SerializedAsset serializedAsset = null;
                if (asset is FlexalonStyle style)
                {
                    var serializedStyle = new SerializedStyle();
                    serializedStyle.properties = style.Properties.ToList();
                    serializedAsset = serializedStyle;
                }
                else if (asset is Texture2D)
                {
                    serializedAsset = new SerializedAsset();
                }
                else if (asset is TMP_FontAsset)
                {
                    serializedAsset = new SerializedAsset();
                }

                if (serializedAsset != null)
                {
                    Log.Verbose($"Serializing asset {asset.GetType().Name} {asset.name}");
                    serializedAsset.id = guid;
                    serializedAsset.type = asset.GetType().Name;
                    serializedAsset.name = asset.name;
                    result.Add(serializedAsset);
                }
            }

            return result;
        }

        public static bool IsSupportedAsset(UnityEngine.Object obj)
        {
            return obj && AssetDatabase.Contains(obj) && IsSupportedAssetType(obj.GetType());
        }

        public static bool IsSupportedAssetType(Type type)
        {
            return type == typeof(FlexalonStyle) || type == typeof(Texture2D) || type == typeof(TMP_FontAsset);
        }
    }
}

#endif