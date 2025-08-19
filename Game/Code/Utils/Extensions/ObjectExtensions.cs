using System.Runtime.CompilerServices;

namespace Game.Utils
{
    public static class ObjectExtensions
    {
        #region STATIC METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToStringWithNull(this object obj, object target = null, string strNull = "null")
        {
            return (obj == null ? strNull : target == null ? obj.ToString() : target.ToString()) ?? string.Empty;
        }

        public static void CallFunction(this object obj, string functionName, object[] functionArgs)
        {
            var method = obj.GetType().GetMethod(functionName);
            if (method != null)
                method.Invoke(obj, functionArgs);
        }
        #endregion
    }
}