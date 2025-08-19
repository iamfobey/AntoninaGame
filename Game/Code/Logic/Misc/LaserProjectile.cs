using Godot;

namespace Game.Logic.Misc
{
    public partial class LaserProjectile : Projectile
    {
        #region PUBLIC METHODS
        public override void Initialize()
        {
            base.Initialize();

            Line.AddPoint(Vector2.Zero);
            Line.AddPoint(Direction * Length);
            ((RectangleShape2D)CollisionShape.GetShape()).Size = new Vector2(Length, 10);

            CollisionShape.Position = Direction * Length / 2;
            CollisionShape.Rotation = Direction.Angle();

            Speed = 0.0f;
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public Line2D Line;
        #endregion

        #region PUBLIC FIELDS
        public float Length = 100.0f;
        #endregion
    }
}