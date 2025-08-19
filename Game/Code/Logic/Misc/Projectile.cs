using Game.Utils;
using Godot;

namespace Game.Logic.Misc
{
    public partial class Projectile : Node2D
    {
        #region DELEGATES
        [Signal]
        public delegate void OnAreaBodyEnteredEventHandler(Projectile self, Node2D body);

        [Signal]
        public delegate void OnAreaBodyExitedEventHandler(Projectile self, Node2D body);

        [Signal]
        public delegate void OnLifeTimeoutEventHandler(Projectile self);
        #endregion

        #region GAME METHODS
        [GameMethod]
        public override void _ProcessGame(double delta)
        {
            GlobalPosition += Direction.Normalized() * Speed * (float)delta;
        }
        #endregion

        #region SIGNAL METHODS
        [SignalMethod]
        public void _OnLifeTimerTimeout()
        {
            EmitSignal(SignalName.OnLifeTimeout, this);

            LifeTimer?.Stop();

            QueueFree();
        }

        [SignalMethod]
        public virtual void _OnAreaBodyEntered(Node2D body)
        {
            EmitSignal(SignalName.OnAreaBodyEntered, this, body);
        }

        [SignalMethod]
        public virtual void _OnAreaBodyExited(Node2D body)
        {
            EmitSignal(SignalName.OnAreaBodyExited, this, body);
        }
        #endregion

        #region PUBLIC METHODS
        public virtual void Initialize()
        {
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public Timer LifeTimer;
        [Export]
        public CollisionShape2D CollisionShape;
        #endregion

        #region PUBLIC FIELDS
        public Vector2 Direction;
        public float Speed = 500.0f;
        #endregion
    }
}