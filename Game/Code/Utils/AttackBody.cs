using Godot;

namespace Game.Utils
{
    public partial class AttackBody : StaticBody2D
    {
        #region EDITOR FIELDS
        [ExportGroup("Base")]
        [Export]
        public Node2D Root;

        [ExportGroup("Logic")]
        [Export]
        public string Type;
        #endregion
    }
}