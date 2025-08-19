using Game.Core.FSM;
using Game.Utils;
using Godot;

namespace Game.Logic.Player.FSM
{
    [Tool]
    public partial class BlockState : State
    {
        #region FSM STATE METHODS
        [StateMethod]
        public override void _StateReady()
        {
            base._StateReady();

            _character = Root<Character>();

            _timer = this.CreateAndAddChild(new SpineAnimationTimer(Callable.From(_OnBlockAnimationCompleted)));
        }

        [StateMethod]
        public override void _StateEnter()
        {
            base._StateEnter();

            _character.SpineAnimationManager.SetAnimationByDirection("Block", _character.PrevMoveComponent.Direction);

            _character.HealthComponent.CanReceiveDamage = false;
            _character.CanUpdateInput = false;
            _character.MoveComponent.Speed = 0.0f;

            _timer.Start(_character.SpineAnimationManager);
        }

        [StateMethod]
        public override void _StateExit()
        {
            base._StateExit();

            _timer.Stop();
        }
        #endregion

        #region SIGNAL METHODS
        [SignalMethod]
        public void _OnBlockAnimationCompleted()
        {
            _character.HealthComponent.CanReceiveDamage = true;

            if (!_character.MoveComponent.IsStunned)
                _character.CanUpdateInput = true;
        }
        #endregion

        #region PRIVATE FIELDS
        private SpineAnimationTimer _timer;
        private Character _character;
        #endregion
    }
}