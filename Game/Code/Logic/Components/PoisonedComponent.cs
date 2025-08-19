using Game.Plugins.CRR;
using Game.Utils;
using Godot;
using Godot.Collections;

namespace Game.Logic.Components
{
    [RegisterEditorResource("PoisonedComponent", nameof(Node2D), nameof(Node2D), IconType.Editor)]
    public partial class PoisonedComponent : Node2D
    {
        #region SIGNAL METHODS
        [SignalMethod]
        public void _OnHealthComponentApplyDamage(Node from, float value, Dictionary parameters)
        {
            HitCount++;
        }
        #endregion

        #region PUBLIC METHODS
        public void Apply(Node2D node)
        {
            IsPoisoned = true;
            node.Modulate = new Color(0.286f, 0.984f, 0.161f);
        }

        public void Undo(Node2D node)
        {
            IsPoisoned = false;
            node.Modulate = new Color(1.0f, 1.0f, 1.0f);
        }
        #endregion

        #region PUBLIC FIELDS
        public bool IsPoisoned;

        public int HitCount = 0;
        #endregion
    }
}