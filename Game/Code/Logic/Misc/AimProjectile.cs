using Game.Logic.Player;
using Game.Utils;
using Godot;
using Godot.Collections;

namespace Game.Logic.Misc
{
    public partial class AimProjectile : Projectile
    {
        #region DELEGATES
        #region Delegates
        [Signal]
        public delegate void OnTweenFinishedEventHandler(Projectile self, Array<Node2D> bodies);
        #endregion
        #endregion

        #region GAME METHODS
        [GameMethod]
        public override void _ReadyGame()
        {
            LifeTimer.Start();
        }

        [GameMethod]
        public override void _ProcessGame(double delta)
        {
            base._ProcessGame(delta);

            Direction = GlobalPosition.DirectionTo(Character.Instance.GlobalPosition);
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public Area2D Area;
        #endregion

        #region PRIVATE FIELDS
        private Tween _tween;
        private Vector2 _backupCollisionScale;
        private Vector2 _backupSpriteScale;
        #endregion
    }
}