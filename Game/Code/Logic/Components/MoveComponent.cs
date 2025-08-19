using System;
using Game.Plugins.CRR;
using Godot;
using Godot.Collections;

namespace Game.Logic.Components
{
    [RegisterEditorResource("MoveComponent", nameof(Node2D), nameof(Node2D), IconType.Editor)]
    public partial class MoveComponent : Node2D
    {
        #region DELEGATES
        [Signal]
        public delegate void OnApplyStunEventHandler(Node from, float value, Dictionary parameters);

        [Signal]
        public delegate void OnStunTimeoutEventHandler();
        #endregion

        #region PUBLIC METHODS
        public void Move(float speed)
        {
            if (IsStunned)
            {
                Velocity = Vector2.Zero;
                Speed = 0.0f;
                return;
            }

            Speed = speed;

            Velocity = Direction * speed;
        }

        public void ApplyStun(Node from, float time, Action timeoutAction = null, Dictionary parameters = null)
        {
            IsStunned = true;
            Velocity = Vector2.Zero;
            Speed = 0.0f;
            GetTree().CreateTimer(time).Timeout += () =>
            {
                if (timeoutAction != null)
                    timeoutAction();
                IsStunned = false;
                EmitSignalOnStunTimeout();
            };

            EmitSignalOnApplyStun(from, time, parameters);
        }
        #endregion

        #region PUBLIC FIELDS
        public Vector2 Direction;
        public Vector2 Velocity;

        public float Speed
        {
            get => _speed;
            set
            {
                if (value == 0.0f)
                {
                    Velocity = Vector2.Zero;
                }
                _speed = value;
            }
        }

        public bool IsStunned = false;
        #endregion

        #region PRIVATE FIELDS
        private float _speed = 0.0f;
        #endregion
    }
}