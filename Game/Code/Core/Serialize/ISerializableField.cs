using System;

namespace Game.Core.Serialize
{
    public interface ISerializableField
    {
        #region PUBLIC METHODS
        /// <summary>
        ///     Determines if this serializer can handle the given type.
        /// </summary>
        bool CanHandle(Type type);

        /// <summary>
        ///     Serializes the given value.
        ///     The returned object should be directly serializable by System.Text.Json.
        ///     (e.g., primitives, POCOs, List<object>, Dictionary<string, object>).
        /// </summary>
        object Serialize(object value);

        /// <summary>
        ///     Deserializes the given System.Text.Json.JsonElement to the targetType.
        /// </summary>
        /// <param name="serializedValue">The JsonElement representing the serialized data.</param>
        /// <param name="targetType">The target type to deserialize into.</param>
        /// <returns>The deserialized object, or null/default if deserialization fails.</returns>
        object Deserialize(object serializedValue, Type targetType);
        #endregion
    }
}