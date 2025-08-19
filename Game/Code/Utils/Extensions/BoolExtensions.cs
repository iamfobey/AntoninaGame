using System;
using System.Runtime.CompilerServices;

namespace Game.Utils
{
    public static class BoolExtensions
    {
        #region STATIC METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WhenTrueToFalse(this ref bool self, Action action)
        {
            if (self)
            {
                action.Invoke();
                self = false;
            }
        }
        #endregion
    }
}