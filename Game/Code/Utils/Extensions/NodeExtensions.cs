using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using ZLinq;

namespace Game.Utils
{
    public static class NodeExtensions
    {
        #region STATIC METHODS
        public static T CreateAndAddChild<T>(this Node self, T node, string name = "") where T : Node
        {
            if (node == null)
                return null;

            if (name != "")
                node.Name = name;
            self.AddChild(node, true);
            node.Owner = Engine.IsEditorHint() ? self.GetTree().EditedSceneRoot : self.GetTree().Root;

            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Node> GetAllNodes(this Node self)
        {
            return self.GetTree().GetAllNodes();
        }

        public static void SetActive(this Node self, bool value, bool visible, bool recursive = false)
        {
            if (self is CanvasItem canvasItem)
                canvasItem.SetDeferred(CanvasItem.PropertyName.Visible, visible);

            self.CallDeferred(Node.MethodName.SetProcess, value);
            self.CallDeferred(Node.MethodName.SetPhysicsProcess, value);
            self.CallDeferred(Node.MethodName.SetProcessInput, value);
            self.CallDeferred(Node.MethodName.SetProcessUnhandledInput, value);
            self.CallDeferred(Node.MethodName.SetProcessUnhandledKeyInput, value);
            self.CallDeferred(Node.MethodName.SetProcessShortcutInput, value);

            if (recursive)
            {
                var children = self.Children().ToList();

                Parallel.ForEach(children, child =>
                {
                    child.SetActive(value, visible);
                });
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DelayFunction(this Node self, float time, Action<Node> timeout)
        {
            self.GetTree().CreateTimer(time).Timeout += () => { timeout.Invoke(self); };
        }
        #endregion
    }
}