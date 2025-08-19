using Game.Core.FSM;
using Game.Utils;
using Godot;

namespace Game.Logic.AI.Attacker.Arc.FSM
{
    [Tool]
    public partial class RunState : State
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
        }

        [StateMethod]
        public override void _StateProcess(double delta)
        {
            base._StateProcess(delta);

            _character.SpineAnimationManager.SetAnimationByDirection("Run", _character.MoveComponent.Direction);
        }

        [StateMethod]
        public override void _StateProcessPhysics(double delta)
        {
            base._StateProcessPhysics(delta);

            if (_character.IsNavFinished())
            {
                _character.CanChangeNavigationPosition = true;
            }

            _character.MoveComponent.Move(_character.RunSpeed);
        }
        #endregion

        #region PRIVATE FIELDS
        private Character _character;
        private int _spawnedDirtCount;
        #endregion
    }
}