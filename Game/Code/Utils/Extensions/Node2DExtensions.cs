using System.Runtime.CompilerServices;
using Godot;

namespace Game.Utils
{
    public static class Node2DExtensions
    {
        #region STATIC METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Teleport(this Node2D self, Node2D target, bool saveY = true)
        {
            self.Position = new Vector2(target.GlobalPosition.X, saveY ? self.GlobalPosition.Y : target.GlobalPosition.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Teleport(this Node2D self, string path, bool saveY = true)
        {
            self.Teleport(self.GetNode<Node2D>(path), saveY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceToPlayer(this Node2D self)
        {
            return self.GlobalPosition.DistanceTo(Logic.Player.Character.Instance.GlobalPosition);
        }
        #endregion
    }
}