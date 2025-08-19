using System;

namespace Game.Utils
{
    [AttributeUsage(AttributeTargets.Field)]
    public partial class SyncVarAttribute(string targetComponentFieldName, string targetPropertyName = "") : Attribute
    {
        #region PUBLIC FIELDS
        public string TargetComponentFieldName = targetComponentFieldName;
        public string TargetPropertyName = targetPropertyName;
        #endregion
    }
}