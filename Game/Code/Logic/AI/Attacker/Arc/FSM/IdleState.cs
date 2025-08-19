using Game.Core.FSM;
using Game.Utils;
using Godot;

namespace Game.Logic.AI.Attacker.Arc.FSM
{
    [Tool]
    public partial class IdleState : State
    {
        #region FSM STATE METHODS
        [StateMethod]
        public override void _StateReady()
        {
            base._StateReady();

            _character = Root<Character>();
        }

        [StateMethod]
        public override void _StateEnter()
        {
            base._StateEnter();

            _character.SpineAnimationManager.SetAnimationByDirection("Idle", _character.MoveComponent.Direction);

            _character.MoveComponent.Speed = 0.0f;
        }
        #endregion

        #region PRIVATE FIELDS
        private Character _character;
        #endregion
    }
}