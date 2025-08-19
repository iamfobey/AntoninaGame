using System;

namespace Game.Plugins.CRR
{
    public enum IconType
    {
        Custom,
        Editor
    }

    [AttributeUsage(AttributeTargets.Class)]
    public partial class RegisterEditorResourceAttribute(
        string name,
        string baseType = "",
        string iconPath = "",
        IconType iconType = IconType.Custom
    )
        : Attribute
    {
        #region PUBLIC FIELDS
        public string BaseType = baseType;
        public string IconPath = iconPath;
        public IconType IconType = iconType;

        public string Name = name;
        #endregion
    }
}