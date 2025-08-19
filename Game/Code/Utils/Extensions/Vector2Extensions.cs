using Godot;

namespace Game.Utils
{
    public static class Vector2Extensions
    {
        #region STATIC METHODS
        public static Vector2 Rnd(float min, float max)
        {
            return new Vector2((float)GD.RandRange(min, max), (float)GD.RandRange(min, max));
        }

        public static Vector2 Rnd(Vector2 min, Vector2 max)
        {
            return new Vector2((float)GD.RandRange(min.X, max.X), (float)GD.RandRange(min.Y, max.Y));
        }

        public static Vector2 NormalizedSafe(this Vector2 vec)
        {
            return vec.IsZeroApprox() ? Vector2.Zero : vec.Normalized();
        }

        public static Vector2 LimitLength(this Vector2 vec, float maxLength)
        {
            if (maxLength < 0) maxLength = 0;
            float lenSq = vec.LengthSquared();
            if (lenSq > maxLength * maxLength)
            {
                return vec.Normalized() * maxLength;
            }
            return vec;
        }
        #endregion
    }
}