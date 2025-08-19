using System.Runtime.CompilerServices;
using Game.Utils;
using Godot;
#if IMGUI
using ImGuiNET;
#endif

namespace Game.Core.Input
{
    public partial class Server : Node
    {
        #region GAME METHODS
        [GameMethod]
        public override void _EnterTreeGame()
        {
            base._EnterTreeGame();

            Instance = this;
        }
        #endregion

        #region STATIC METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsActionPressed(string action, bool ignoreImGui = false)
        {
            #if IMGUI
            if (!ignoreImGui && ImGui.GetIO().WantCaptureMouse)
                return false;
            #endif

            return Godot.Input.IsActionPressed(action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsActionJustPressed(string action, bool ignoreImGui = false)
        {
            #if IMGUI
            if (!ignoreImGui && ImGui.GetIO().WantCaptureMouse)
                return false;
            #endif

            return Godot.Input.IsActionJustPressed(action);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsActionJustReleased(string action, bool ignoreImGui = false)
        {
            #if IMGUI
            if (!ignoreImGui && ImGui.GetIO().WantCaptureMouse)
                return false;
            #endif

            return Godot.Input.IsActionJustReleased(action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ActionPress(string action, float strength = 1.0f)
        {
            Godot.Input.ActionPress(action, strength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ActionRelease(string action)
        {
            Godot.Input.ActionRelease(action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetVector(string negativeX, string positiveX, string negativeY, string positiveY,
            float deadzone = -1.0f, bool ignoreImGui = false)
        {
            #if IMGUI
            if (!ignoreImGui && ImGui.GetIO().WantCaptureMouse)
                return Vector2.Zero;
            #endif

            return Godot.Input.GetVector(negativeX, positiveX, negativeY, positiveY, deadzone);
        }
        #endregion

        #region STATIC FIELDS
        public static Server Instance { get; private set; }
        #endregion
    }
}