using Game.Utils;
using Godot;

namespace Game.Core.UI
{
    public partial class TouchScreenVirtualJoystick : Control
    {
        #region ENUMS
        public enum EJoystickMode
        {
            Fixed,
            Dynamic,
            Following
        }

        public enum EVisibilityMode
        {
            Always,
            TouchscreenOnly,
            WhenTouched
        }
        #endregion

        #region GAME METHODS
        [GameMethod]
        public override void _ReadyGame()
        {
            _base = GetNode<TextureRect>("Base");
            _tip = GetNode<TextureRect>("Base/Tip");
            _baseDefaultPosition = _base.Position;
            _tipDefaultPosition = _tip.Position;
            _defaultColor = _tip.Modulate;

            if (!DisplayServer.IsTouchscreenAvailable() && VisibilityMode == EVisibilityMode.TouchscreenOnly)
                Hide();

            if (VisibilityMode == EVisibilityMode.WhenTouched)
                Hide();
        }

        [GameMethod]
        public override void _InputGame(InputEvent @event)
        {
            if (@event is InputEventScreenTouch eventTouch)
            {
                if (eventTouch.Pressed)
                {
                    if (IsPointInsideJoystickArea(eventTouch.Position) && _touchIndex == -1)
                    {
                        if (JoystickMode == EJoystickMode.Dynamic || JoystickMode == EJoystickMode.Following ||
                            JoystickMode == EJoystickMode.Fixed && IsPointInsideBase(eventTouch.Position))
                        {
                            if (JoystickMode == EJoystickMode.Dynamic || JoystickMode == EJoystickMode.Following)
                                MoveBase(eventTouch.Position);

                            if (VisibilityMode == EVisibilityMode.WhenTouched)
                                Show();

                            _touchIndex = eventTouch.Index;
                            _tip.Modulate = PressedColor;

                            UpdateJoystick(eventTouch.Position);
                        }
                    }
                }
                else if (eventTouch.Index == _touchIndex)
                {
                    ResetJoystick();
                    if (VisibilityMode == EVisibilityMode.WhenTouched)
                        Hide();
                }
            }
            else if (@event is InputEventScreenDrag eventDrag)
            {
                if (eventDrag.Index == _touchIndex)
                    UpdateJoystick(eventDrag.Position);
            }
        }
        #endregion

        #region PRIVATE METHODS
        private void MoveBase(Vector2 newPosition)
        {
            _base.GlobalPosition = newPosition - _base.PivotOffset * GetGlobalTransformWithCanvas().Scale;
        }

        private void MoveTip(Vector2 newPosition)
        {
            _tip.GlobalPosition = newPosition - _tip.PivotOffset * _base.GetGlobalTransformWithCanvas().Scale;
        }

        private bool IsPointInsideJoystickArea(Vector2 point)
        {
            bool x = point.X >= GlobalPosition.X && point.X <= GlobalPosition.X + Size.X * GetGlobalTransformWithCanvas().Scale.X;
            bool y = point.Y >= GlobalPosition.Y && point.Y <= GlobalPosition.Y + Size.Y * GetGlobalTransformWithCanvas().Scale.Y;

            return x && y;
        }

        private Vector2 GetBaseRadius()
        {
            return _base.Size * _base.GetGlobalTransformWithCanvas().Scale / 2;
        }

        private bool IsPointInsideBase(Vector2 point)
        {
            var baseRadius = GetBaseRadius();
            var center = _base.GlobalPosition + baseRadius;

            return (point - center).LengthSquared() <= baseRadius.X * baseRadius.X;
        }

        private void UpdateJoystick(Vector2 touchPosition)
        {
            var baseRadius = GetBaseRadius();
            var center = _base.GlobalPosition + baseRadius;
            var vector = (touchPosition - center).LimitLength(ClampZoneSize);

            if (JoystickMode == EJoystickMode.Following && touchPosition.DistanceTo(center) > ClampZoneSize)
                MoveBase(touchPosition - vector);

            MoveTip(center + vector);

            if (vector.LengthSquared() > DeadZoneSize * DeadZoneSize)
            {
                IsPressed = true;
                InputDirection = (vector - vector.Normalized() * DeadZoneSize) / (ClampZoneSize - DeadZoneSize);
            }
            else
            {
                IsPressed = false;
                InputDirection = Vector2.Zero;
            }

            if (UseInputActions)
                HandleInputActions();
        }

        private void HandleInputActions()
        {
            if (InputDirection.X >= 0 && Input.Server.IsActionPressed(ActionLeft))
                Input.Server.ActionRelease(ActionLeft);
            if (InputDirection.X <= 0 && Input.Server.IsActionPressed(ActionRight))
                Input.Server.ActionRelease(ActionRight);
            if (InputDirection.Y >= 0 && Input.Server.IsActionPressed(ActionUp))
                Input.Server.ActionRelease(ActionUp);
            if (InputDirection.Y <= 0 && Input.Server.IsActionPressed(ActionDown))
                Input.Server.ActionRelease(ActionDown);

            if (InputDirection.X < 0)
                Input.Server.ActionPress(ActionLeft, -InputDirection.X);
            if (InputDirection.X > 0)
                Input.Server.ActionPress(ActionRight, InputDirection.X);
            if (InputDirection.Y < 0)
                Input.Server.ActionPress(ActionUp, -InputDirection.Y);
            if (InputDirection.Y > 0)
                Input.Server.ActionPress(ActionDown, InputDirection.Y);
        }

        private void ResetJoystick()
        {
            IsPressed = false;
            InputDirection = Vector2.Zero;

            _touchIndex = -1;
            _tip.Modulate = _defaultColor;
            _base.Position = _baseDefaultPosition;
            _tip.Position = _tipDefaultPosition;

            if (UseInputActions)
                foreach (string action in new[] { ActionLeft, ActionRight, ActionUp, ActionDown })
                    Input.Server.ActionRelease(action);
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Parameters")]
        [ExportGroup("Logic")]
        [Export]
        public string ActionDown = "ui_down";
        [Export]
        public string ActionLeft = "ui_left";
        [Export]
        public string ActionRight = "ui_right";
        [Export]
        public string ActionUp = "ui_up";
        [Export(PropertyHint.Range, "0, 500, 1")]
        public float ClampZoneSize = 75.0f;
        [Export(PropertyHint.Range, "0, 200, 1")]
        public float DeadZoneSize = 10.0f;
        [Export]
        public EJoystickMode JoystickMode = EJoystickMode.Fixed;
        [Export]
        public Color PressedColor = Colors.Gray;
        [Export]
        public bool UseInputActions = true;
        [Export]
        public EVisibilityMode VisibilityMode = EVisibilityMode.Always;
        #endregion

        #region PUBLIC FIELDS
        public Vector2 InputDirection = Vector2.Zero;
        public bool IsPressed = false;
        #endregion

        #region PRIVATE FIELDS
        private TextureRect _base;
        private Vector2 _baseDefaultPosition;
        private Color _defaultColor;
        private TextureRect _tip;
        private Vector2 _tipDefaultPosition;
        private int _touchIndex = -1;
        #endregion
    }
}