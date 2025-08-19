using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Game.Core.Logger;
using Game.Utils;
using Godot;
using ZLinq;

namespace Game.Core.Serialize
{
    public partial class Server : Node
    {
        #region GAME METHODS
        [GameMethod]
        public override void _EnterTreeGame()
        {
            base._EnterTreeGame();

            Instance = this;

            Log.Info("Initializing reflection cache...", ELogCategory.Serialization);

            var assembly = Assembly.GetExecutingAssembly();

            _sSerializableNodeTypesCache = new HashSet<Type>(
                assembly.GetTypes()
                    .Where(type => Attribute.IsDefined(type, typeof(GameSerializableAttribute)))
            );

            _sFieldSerializersCache = assembly.GetTypes()
                .Where(type => typeof(ISerializableField).IsAssignableFrom(type) && type is { IsInterface: false, IsAbstract: false })
                .Select(type => (ISerializableField)Activator.CreateInstance(type))
                .ToList();

            _sSerializerLookupCacheByType = new Dictionary<Type, ISerializableField>();
            Log.Info($"Reflection cache initialized. {_sSerializableNodeTypesCache.Count} serializable types, "
                + $"{_sFieldSerializersCache.Count} field serializers found.", ELogCategory.Serialization);
        }
        #endregion

        #region STATIC METHODS
        private static ISerializableField GetSerializerForType(Type fieldType)
        {
            if (_sSerializerLookupCacheByType.TryGetValue(fieldType, out var cachedSerializer))
            {
                return cachedSerializer;
            }

            var serializer = _sFieldSerializersCache.FirstOrDefault(s => s.CanHandle(fieldType));
            if (serializer != null)
            {
                _sSerializerLookupCacheByType[fieldType] = serializer; // Cache it
            }
            return serializer;
        }

        private static void CollectAllNodesRecursively(Node parent, List<Node> nodesList)
        {
            if (parent == null) return;
            nodesList.Add(parent);
            foreach (var child in parent.Children())
            {
                CollectAllNodesRecursively(child, nodesList);
            }
        }
        #endregion

        #region PUBLIC METHODS
        public string SerializeAllNodesToString()
        {
            var sceneTree = GetTree();
            if (sceneTree == null)
            {
                Log.Error("SceneTree is null in SerializeAllNodesToString.", ELogCategory.Serialization);
                return "{}";
            }

            List<Node> allNodes = [];
            CollectAllNodesRecursively(sceneTree.Root, allNodes);

            foreach (var node in allNodes)
            {
                if (node is IGameSerializeAware serializeAware)
                {
                    try
                    {
                        serializeAware._OnBeforeSerialize();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error in _OnBeforeSerialize for node {node.GetPath()}: {ex.Message}", ELogCategory.Serialization);
                    }
                }
            }

            var serializableNodesData = new Dictionary<string, Dictionary<string, object>>();

            foreach (var node in allNodes)
            {
                if (node != null && _sSerializableNodeTypesCache.Contains(node.GetType()))
                {
                    string nodePath = node.GetPath().ToString();
                    var serializedNodeContent = SerializeNodeToDictionary(node);
                    if (serializedNodeContent.Any())
                    {
                        serializableNodesData[nodePath] = serializedNodeContent;
                    }
                }
            }

            string jsonData = JsonSerializer.Serialize(serializableNodesData, _jsonOptions);

            foreach (var node in allNodes)
            {
                if (node is IGameSerializeAware serializeAware)
                {
                    try
                    {
                        serializeAware._OnSerialize();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error in _OnSerialize for node {node.GetPath()}: {ex.Message}", ELogCategory.Serialization);
                    }
                }
            }

            return jsonData;
        }

        public void DeserializeAllNodesFromString(string jsonData)
        {
            var sceneTree = GetTree();
            if (sceneTree == null)
            {
                Log.Error("SceneTree is null in DeserializeAllNodesFromString.", ELogCategory.Serialization);
                return;
            }

            if (string.IsNullOrWhiteSpace(jsonData))
            {
                Log.Warn("jsonData is null or empty.", ELogCategory.Serialization);
                return;
            }

            Dictionary<string, Dictionary<string, JsonElement>> allNodesData;
            try
            {
                allNodesData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, JsonElement>>>(jsonData, _jsonOptions);
            }
            catch (JsonException ex)
            {
                Log.Error($"Failed to deserialize main JSON data: {ex.Message}", ELogCategory.Serialization);
                return;
            }

            if (allNodesData == null) return;

            List<Node> allCurrentNodesInScene = [];
            CollectAllNodesRecursively(sceneTree.Root, allCurrentNodesInScene);

            foreach (var node in allCurrentNodesInScene)
            {
                if (node is IGameSerializeAware serializeAware)
                {
                    try
                    {
                        serializeAware._OnBeforeDeserialize();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error in _OnBeforeDeserialize for node {node.GetPath()}: {ex.Message}", ELogCategory.Serialization);
                    }
                }
            }

            foreach (var entry in allNodesData)
            {
                var path = new NodePath(entry.Key);
                var serializedNodeFields = entry.Value;

                var node = sceneTree.Root.GetNodeOrNull(path);
                if (node != null)
                {
                    if (_sSerializableNodeTypesCache.Contains(node.GetType()))
                    {
                        DeserializeNodeFromDictionary(node, serializedNodeFields);
                    }
                    else
                    {
                        Log.Warn($"Node at path {path} is not marked as [Serializable] or its type changed."
                            + $" Skipping deserialization.", ELogCategory.Serialization);
                    }
                }
                else
                {
                    Log.Warn($"Node not found at path: {path} during deserialization.", ELogCategory.Serialization);
                }
            }

            foreach (var node in allCurrentNodesInScene)
            {
                if (node is IGameSerializeAware serializeAware)
                {
                    try
                    {
                        serializeAware._OnDeserialize();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error in _OnDeserialize for node {node.GetPath()}: {ex.Message}", ELogCategory.Serialization);
                    }
                }
            }
        }
        #endregion

        #region PRIVATE METHODS
        private Dictionary<string, object> SerializeNodeToDictionary(Node node)
        {
            var serializedData = new Dictionary<string, object>();
            var members = GetAllSerializableMembers(node.GetType());

            foreach (var memberInfo in members)
            {
                var attribute = (GameSerializeAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(GameSerializeAttribute));
                if (attribute == null) continue;

                Type memberType;
                object value;
                string memberName = memberInfo.Name;

                if (memberInfo is FieldInfo fieldInfo)
                {
                    memberType = fieldInfo.FieldType;
                    value = fieldInfo.GetValue(node);
                }
                else if (memberInfo is PropertyInfo propertyInfo)
                {
                    if (!propertyInfo.CanRead)
                    {
                        Log.Warn($"Property '{memberName}' on '{node.GetPath()}' is marked"
                            + $" [Serialize] but is not readable. Skipping.", ELogCategory.Serialization);
                        continue;
                    }
                    memberType = propertyInfo.PropertyType;
                    value = propertyInfo.GetValue(node);
                }
                else continue;

                var serializer = GetSerializerForType(memberType);
                if (serializer != null)
                {
                    if (value != null)
                    {
                        serializedData.Add(memberName, serializer.Serialize(value));
                    }
                }
                else
                {
                    Log.Error($"No suitable serializer found for type '{memberType.FullName}'"
                        + $" of member '{memberName}' in node '{node.GetPath()}'.", ELogCategory.Serialization);
                }
            }
            return serializedData;
        }

        private List<MemberInfo> GetAllSerializableMembers(Type type)
        {
            var serializableMembers = new List<MemberInfo>();
            var currentType = type;

            while (currentType != null && currentType != typeof(object)) // Идем вверх до object или null
            {
                // Ищем только объявленные в ЭТОМ типе члены
                var declaredMembers = currentType.GetMembers(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly
                );

                foreach (var memberInfo in declaredMembers)
                {
                    // ЗАМЕНИТЕ GameSerializeAttribute НА ПОЛНОЕ ИМЯ ВАШЕГО АТРИБУТА
                    if (Attribute.IsDefined(memberInfo, typeof(GameSerializeAttribute)))
                    {
                        // Добавляем, только если член с таким именем еще не был добавлен из более производного класса
                        // Это важно, если есть переопределения или сокрытие (shadowing)
                        if (!serializableMembers.Any(m => m.Name == memberInfo.Name))
                        {
                            serializableMembers.Add(memberInfo);
                        }
                    }
                }
                currentType = currentType.BaseType;
            }
            return serializableMembers;
        }

        private void DeserializeNodeFromDictionary(Node node, Dictionary<string, JsonElement> serializedFields)
        {
            var members = GetAllSerializableMembers(node.GetType());

            foreach (var memberInfo in members)
            {
                var attribute = (GameSerializeAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(GameSerializeAttribute));
                if (attribute == null) continue;

                string memberName = memberInfo.Name;

                if (serializedFields.TryGetValue(memberName, out var serializedValueElement))
                {
                    Type memberType;
                    Action<object, object> setValueAction;

                    if (memberInfo is FieldInfo fieldInfo)
                    {
                        memberType = fieldInfo.FieldType;
                        setValueAction = fieldInfo.SetValue;
                    }
                    else if (memberInfo is PropertyInfo propertyInfo)
                    {
                        if (!propertyInfo.CanWrite)
                        {
                            Log.Warn($"Property '{memberName}' on '{node.GetPath()}' is marked [Serialize] but"
                                + $" is not writable. Skipping deserialization for this member.", ELogCategory.Serialization);
                            continue;
                        }
                        memberType = propertyInfo.PropertyType;
                        setValueAction = propertyInfo.SetValue;
                    }
                    else continue;

                    var serializer = GetSerializerForType(memberType);
                    if (serializer != null)
                    {
                        object deserializedValue = serializer.Deserialize(serializedValueElement, memberType);

                        if (deserializedValue == null && memberType.IsValueType && Nullable.GetUnderlyingType(memberType) == null)
                        {
                            Log.Warn($"Serializer for '{memberType.FullName}' returned null for non-nullable member '{memberName}' "
                                + $"on node '{node.GetPath()}'. Using default value for type.", ELogCategory.Serialization);
                            deserializedValue = Activator.CreateInstance(memberType);
                        }

                        try
                        {
                            setValueAction(node, deserializedValue);
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Error setting member '{memberName}' on '{node.GetPath()}' with value "
                                + $"'{deserializedValue}': {ex.Message}", ELogCategory.Serialization);
                        }
                    }
                    else
                    {
                        Log.Error($"No suitable deserializer found for type '{memberType.FullName}' of member '{memberName}' "
                            + $"in node '{node.GetPath()}'.", ELogCategory.Serialization);
                    }
                }
            }
        }
        #endregion

        #region STATIC FIELDS
        private static HashSet<Type> _sSerializableNodeTypesCache;
        private static List<ISerializableField> _sFieldSerializersCache;
        private static Dictionary<Type, ISerializableField> _sSerializerLookupCacheByType;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve,
            Converters = { new JsonStringEnumConverter() }
        };
        public static Server Instance { get; private set; }
        #endregion
    }
}