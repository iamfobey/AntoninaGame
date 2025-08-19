using System.Runtime.CompilerServices;
using Game.Core.Logger;
using Godot;

namespace Game.Utils
{
    public static class NavigationRegion2DExtensions
    {
        #region STATIC METHODS
        public static Vector2 GetRandomPointRadius(this NavigationRegion2D navigationRegion, Vector2 mainPoint, float minDist,
            float maxDist)
        {
            const int maxAttempts = 250;
            for (int i = 0; i < maxAttempts; i++)
            {
                var point = GetRandomPoint(navigationRegion);
                if (mainPoint.DistanceTo(point) > minDist && mainPoint.DistanceTo(point) < maxDist)
                    return point;
            }
            Log.Warn($"Failed to find a random point within the specified distance after {maxAttempts} attempts.", ELogCategory.Utils);
            return mainPoint;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetRandomPoint(this NavigationRegion2D navigationRegion)
        {
            return NavigationServer2D.RegionGetRandomPoint(navigationRegion.GetRid(), 1, false);
        }
        #endregion
    }
}