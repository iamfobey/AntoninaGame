using Game.Utils;
using Godot;

namespace Game.Logic.Misc
{
    public partial class SnotProjectile : Projectile
    {
        #region GAME METHODS
        [GameMethod]
        public override void _ReadyGame()
        {
            _startPosition = GlobalPosition;
            _time = 0;

            float distance = _startPosition.DistanceTo(TargetPosition);
            _duration = distance / Speed;
            _isDescending = false;
        }

        [GameMethod]
        public override void _ProcessGame(double delta)
        {
            _time += (float)delta;
            float t = _time / _duration;

            float x = Mathf.Lerp(_startPosition.X, TargetPosition.X, t);
            float y = Mathf.Lerp(_startPosition.Y, TargetPosition.Y, t) - ArcHeight * Mathf.Sin(t * Mathf.Pi);

            GlobalPosition = new Vector2(x, y);

            if (!_isDescending && y < _startPosition.Y - ArcHeight * 0.5f)
            {
                _isDescending = true;
            }
            else if (_isDescending && Mathf.Abs(y - _startPosition.Y) < 5.0f)
            {
                QueueFree();
            }
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Parameters")]
        [ExportGroup("Logic")]
        [Export]
        public float ArcHeight = 500.0f;
        [Export]
        public Vector2 TargetPosition;
        #endregion

        #region PRIVATE FIELDS
        private Vector2 _startPosition;
        private float _time;
        private float _duration;
        private bool _isDescending;
        #endregion
    }
}