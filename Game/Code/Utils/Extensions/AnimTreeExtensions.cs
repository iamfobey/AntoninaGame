using System.Runtime.CompilerServices;
using Godot;

namespace Game.Utils
{
    public static class AnimationTreeExtensions
    {
        #region STATIC METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnimationTree RequestAnimation(this AnimationTree self, string name, string value)
        {
            self.Set("parameters/" + name + "/transition_request", value);
            return self;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AnimationTree UpdateBlendPosition(this AnimationTree self, string blendType, Vector2 direction)
        {
            self.Set($"parameters/{blendType}BlendSpace2D/blend_position", direction);
            return self;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetBlendPosition<T>(this AnimationTree self, string blendType)
        {
            return self.Get($"parameters/{blendType}BlendSpace2D/blend_position").As<T>();
        }
        #endregion
    }
}