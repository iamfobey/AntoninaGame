using System;
using System.Globalization;
using System.Text.Json;
using Game.Core.Logger;
using Godot;

namespace Game.Core.Serialize
{
    public class IntSerializer : ISerializableField
    {
        #region PUBLIC METHODS
        public bool CanHandle(Type type)
        {
            return type == typeof(int);
        }

        public object Serialize(object value)
        {
            return (int)value;
        }

        public object Deserialize(object serializedValue, Type targetType)
        {
            if (serializedValue is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out int result))
                    return result;

                if (element.ValueKind == JsonValueKind.String && int.TryParse(element.GetString(), NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out result))
                    return result;
            }

            Log.Error($"Failed to deserialize int from '{serializedValue?.GetType().Name}: {serializedValue}'. Returning default(int).",
                ELogCategory.Serialization);

            return 0;
        }
        #endregion
    }

    public class StringSerializer : ISerializableField
    {
        #region PUBLIC METHODS
        public bool CanHandle(Type type)
        {
            return type == typeof(string);
        }

        public object Serialize(object value)
        {
            return value as string;
        }

        public object Deserialize(object serializedValue, Type targetType)
        {
            if (serializedValue is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.String)
                    return element.GetString();

                if (element.ValueKind == JsonValueKind.Null)
                    return null;
            }

            Log.Warn(
                $"Failed to deserialize string from '{serializedValue?.GetType().Name}: {serializedValue}'. Expected string or null. Returning null.",
                ELogCategory.Serialization);

            return null;
        }
        #endregion
    }

    public class FloatSerializer : ISerializableField
    {
        #region PUBLIC METHODS
        public bool CanHandle(Type type)
        {
            return type == typeof(float);
        }

        public object Serialize(object value)
        {
            return (float)value;
        }

        public object Deserialize(object serializedValue, Type targetType)
        {
            if (serializedValue is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Number && element.TryGetSingle(out float result))
                    return result;

                if (element.ValueKind == JsonValueKind.String && float.TryParse(element.GetString(),
                        NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result))
                    return result;
            }

            Log.Error(
                $"Failed to deserialize float from '{serializedValue?.GetType().Name}: {serializedValue}'. Returning default(float).",
                ELogCategory.Serialization);

            return 0;
        }
        #endregion
    }

    public class Vector2Serializer : ISerializableField
    {
        #region PUBLIC METHODS
        public bool CanHandle(Type type)
        {
            return type == typeof(Vector2);
        }

        public object Serialize(object value)
        {
            var vec = (Vector2)value;

            return new { vec.X, vec.Y };
        }

        public object Deserialize(object serializedValue, Type targetType)
        {
            if (serializedValue is JsonElement element && element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("X", out var xElement) && xElement.TryGetSingle(out float x) &&
                    element.TryGetProperty("Y", out var yElement) && yElement.TryGetSingle(out float y))
                {
                    return new Vector2(x, y);
                }
            }

            Log.Error(
                $"Failed to deserialize Vector2 from '{serializedValue?.GetType().Name}: {serializedValue}'. Returning Vector2.Zero.",
                ELogCategory.Serialization);

            return Vector2.Zero;
        }
        #endregion
    }

    public class BoolSerializer : ISerializableField
    {
        #region PUBLIC METHODS
        public bool CanHandle(Type type)
        {
            return type == typeof(bool);
        }

        public object Serialize(object value)
        {
            return (bool)value;
        }

        public object Deserialize(object serializedValue, Type targetType)
        {
            if (serializedValue is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.True)
                    return true;

                if (element.ValueKind == JsonValueKind.False)
                    return false;

                if (element.ValueKind == JsonValueKind.String && bool.TryParse(element.GetString(), out bool result))
                    return result;
            }

            Log.Error(
                $"Failed to deserialize bool from '{serializedValue?.GetType().Name}: {serializedValue}'. Returning default(bool).",
                ELogCategory.Serialization);

            return false;
        }
        #endregion
    }

    public class NodeReferenceSerializer : ISerializableField
    {
        #region PUBLIC METHODS
        public bool CanHandle(Type type)
        {
            return typeof(Node).IsAssignableFrom(type);
        }

        public object Serialize(object value)
        {
            if (value is Node nodeValue)
            {
                if (nodeValue.IsInsideTree())
                {
                    return nodeValue.GetPath().ToString();
                }
                Log.Warn($"Node '{nodeValue.Name}' is not in tree. Cannot get path. Serializing as null.", ELogCategory.Serialization);
                return null;
            }
            Log.Warn($"Value is not a Node or is null. Type: {value?.GetType().FullName}. Serializing as null.",
                ELogCategory.Serialization);
            return null;
        }

        public object Deserialize(object serializedValue, Type targetType)
        {
            if (serializedValue is JsonElement { ValueKind: JsonValueKind.String } element)
            {
                string pathString = element.GetString();
                if (string.IsNullOrEmpty(pathString))
                {
                    Log.Warn("Path string is null or empty. Returning null.", ELogCategory.Serialization);
                    return null;
                }

                var nodePath = new NodePath(pathString);

                if (Server.Instance == null || Server.Instance.GetTree() == null)
                {
                    Log.Error("Server.Instance or SceneTree is not available. Cannot get node by path.", ELogCategory.Serialization);
                    return null;
                }

                var resolvedNode = Server.Instance.GetTree().Root.GetNodeOrNull(nodePath);

                if (resolvedNode == null)
                {
                    Log.Warn($"Node not found at path '{pathString}'. Returning null.", ELogCategory.Serialization);
                    return null;
                }

                if (targetType.IsAssignableFrom(resolvedNode.GetType()))
                {
                    return resolvedNode; // Возвращаем найденный узел
                }
                Log.Error($"Node at path '{pathString}' is of type '{resolvedNode.GetType().FullName}', " +
                    $"which is not assignable to target type '{targetType.FullName}'. Returning null.", ELogCategory.Serialization);
                return null;
            }
            if (serializedValue is JsonElement el && el.ValueKind == JsonValueKind.Null)
            {
                return null; // Если было сохранено как null
            }

            Log.Warn(
                $"Serialized value is not a string path. Type: {serializedValue?.GetType().Name}. Value: {serializedValue}. Returning null.",
                ELogCategory.Serialization);
            return null;
        }
        #endregion
    }
}