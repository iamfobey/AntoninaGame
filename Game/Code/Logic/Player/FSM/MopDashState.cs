using Game.Core.FSM;
using Game.Core.Input;
using Game.Utils;
using Godot;

namespace Game.Logic.Player.FSM
{
    [Tool]
    public partial class MopDashState : State
    {
        #region FSM STATE METHODS
        [StateMethod]
        public override void _StateReady()
        {
            base._StateReady();

            _character = Root<Character>();
        }

        [StateMethod]
        public override void _StateInitialize()
        {
            base._StateInitialize();
        }

        [StateMethod]
        public override void _StateEnter()
        {
            base._StateEnter();

            _tweenedSpeed = _character.DashSpeed;

            var tween = CreateTween();
            tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
            tween.TweenProperty(this, nameof(_tweenedSpeed), _character.MopDashSpeed, 0.5f);

            _character.IgnoreDirectionBuffer = true;
        }

        [StateMethod]
        public override void _StateProcess(double delta)
        {
            base._StateProcess(delta);

            _character.SpineAnimationManager.SetAnimationByDirection("Speed", _character.PrevMoveComponent.Direction);
        }

        [StateMethod]
        public override void _StateExit()
        {
            base._StateExit();

            _character.IgnoreDirectionBuffer = false;
        }

        [StateMethod]
        public override void _StateProcessPhysics(double delta)
        {
            base._StateProcessPhysics(delta);

            if (_character.MoveComponent.Direction == Vector2.Zero)
            {
                FSM.RequestState<IdleState>();
                return;
            }

            if (Server.IsActionJustReleased(Map.PlayerRun, true) || !_character.StaminaComponent.HasEnoughStamina(_character.MopDashStamina))
            {
                FSM.RequestState<WalkState>();
                return;
            }

            _character.MoveComponent.Move(_tweenedSpeed);
            _character.StaminaComponent.UseStamina(_character, _character.MopDashStamina * (float)delta);
        }
        #endregion

        #region PRIVATE FIELDS
        private float _tweenedSpeed = 0.0f;
        private Character _character;
        #endregion
    }
}