using System;
using System.Runtime.CompilerServices;

namespace Game.Utils
{
    public static class ByteSize
    {
        #region ENUMS
        public enum Units
        {
            Byte,
            Kb,
            Mb,
            Gb,
            Tb,
            Pb,
            Eb,
            Zb,
            Yb
        }
        #endregion

        #region STATIC METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToSize(this long value, Units units)
        {
            return (value / Math.Pow(1024, (long)units)).ToString("0.00");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToSize(this double value, Units units)
        {
            return (value / Math.Pow(1024, (long)units)).ToString("0.00");
        }
        #endregion
    }
}