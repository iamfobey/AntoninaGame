using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
using ZLinq;

namespace Game.Utils
{
    public static class TreeExtensions
    {
        #region STATIC METHODS
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Node> GetAllNodes(this SceneTree self)
        {
            return GetAllNodesImpl(self.Root);
        }

        private static List<Node> GetAllNodesImpl(Node node)
        {
            var nodes = new List<Node>
            {
                node
            };

            foreach (var child in node.Children())
                nodes.AddRange(GetAllNodesImpl(child));

            return nodes;
        }
        #endregion
    }
}