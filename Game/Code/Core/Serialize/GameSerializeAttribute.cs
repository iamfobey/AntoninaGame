using System;

namespace Game.Core.Serialize
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class GameSerializeAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class GameSerializableAttribute : Attribute
    {
    }
}