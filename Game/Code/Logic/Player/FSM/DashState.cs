using Game.Core.FSM;
using Game.Core.Input;
using Game.Utils;
using Godot;

namespace Game.Logic.Player.FSM
{
    [Tool]
    public partial class DashState : State
    {
        #region FSM STATE METHODS
        [StateMethod]
        public override void _StateReady()
        {
            base._StateReady();

            _character = Root<Character>();

            _timer = this.CreateAndAddChild(new SpineAnimationTimer(Callable.From(_OnDashAnimationCompleted)));
        }

        [StateMethod]
        public override void _StateEnter()
        {
            base._StateEnter();

            _character.SpineAnimationManager.SetAnimationByDirection("Speed", _character.PrevMoveComponent.Direction);

            _direction = _character.PrevMoveComponent.Direction;

            _character.StaminaComponent.UseStamina(_character, _character.DashStamina);
            _character.HealthComponent.CanReceiveDamage = false;
            _character.CanUpdateInput = false;

            _character.IgnoreDirectionBuffer = true;

            _timer.Start(_character.SpineAnimationManager, 1.5f);
        }

        [StateMethod]
        public override void _StateProcessPhysics(double delta)
        {
            base._StateProcessPhysics(delta);

            _character.MoveComponent.Velocity = new Vector2(_direction.X * _character.DashSpeed,
                _direction.Y * _character.DashSpeed);
        }

        [StateMethod]
        public override void _StateExit()
        {
            base._StateExit();

            _character.IgnoreDirectionBuffer = false;

            _timer.Stop();
        }
        #endregion

        #region SIGNAL METHODS
        [SignalMethod]
        public void _OnDashAnimationCompleted()
        {
            _character.HealthComponent.CanReceiveDamage = true;

            if (!_character.MoveComponent.IsStunned)
                _character.CanUpdateInput = true;

            if (Server.IsActionPressed(Map.PlayerRun, true))
            {
                FSM.RequestState<MopDashState>();
            }
            else
            {
                FSM.RequestState<WalkState>();
            }
        }
        #endregion

        #region PRIVATE FIELDS
        private SpineAnimationTimer _timer;
        private Character _character;
        private Vector2 _direction;
        #endregion
    }
}