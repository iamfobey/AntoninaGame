using System.Reflection;
using Game.Core.Logger;
using Game.Logic.Components;
using Godot;

namespace Game.Utils
{
    public static class SyncVar
    {
        #region STATIC METHODS
        public static void SyncVariables(this Node node)
        {
            var nodeType = node.GetType();
            var sourceFields = nodeType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var sourceField in sourceFields)
            {
                var syncAttribute = sourceField.GetCustomAttribute<SyncVarAttribute>();
                if (syncAttribute == null)
                    continue;

                var targetComponentField = nodeType.GetField(syncAttribute.TargetComponentFieldName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (targetComponentField == null)
                {
                    Log.Error($"Field '{syncAttribute.TargetComponentFieldName}' not found on node '{node.Name}'.",
                        ELogCategory.Utils);
                    continue;
                }

                object targetComponentObject = targetComponentField.GetValue(node);
                if (targetComponentObject == null)
                {
                    Log.Error(
                        $"The field '{syncAttribute.TargetComponentFieldName}' on node '{node.Name}' is null. "
                        + $"Assign the component in the editor or in code.", ELogCategory.Utils);
                    continue;
                }

                var targetComponentType = targetComponentObject.GetType();

                object sourceValue = sourceField.GetValue(node);

                var targetProperty = targetComponentType.GetProperty(syncAttribute.TargetPropertyName != "" ? syncAttribute.TargetPropertyName :
                    sourceField.Name);
                if (targetProperty != null && targetProperty.CanWrite)
                {
                    targetProperty.SetValue(targetComponentObject, sourceValue);
                    Log.Debug($"{node.Name} synced '{sourceField.Name}' -> '{targetComponentField.Name}.{targetProperty.Name}'", ELogCategory.Utils);
                }
                else
                {
                    var targetField = targetComponentType.GetField(syncAttribute.TargetPropertyName != "" ? syncAttribute.TargetPropertyName :
                        sourceField.Name);
                    if (targetField != null)
                    {
                        targetField.SetValue(targetComponentObject, sourceValue);
                        Log.Debug($"Synced '{sourceField.Name}' -> '{targetComponentField.Name}.{targetField.Name}'", ELogCategory.Utils);
                    }
                    else
                    {
                        Log.Error(
                            $"SyncVar Error: Property or Field '{syncAttribute.TargetPropertyName}' not found or is not writable on component '{targetComponentType.Name}'.", ELogCategory.Utils);
                    }
                }
            }
        }
        #endregion
    }
}