using Game.Core.FSM;
using Game.Utils;
using Godot;

namespace Game.Logic.AI.Attacker.Aimer.FSM
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

            _character.AnimationTree.RequestAnimation("Transition", "Run");
        }

        [StateMethod]
        public override void _StateProcess(double delta)
        {
            base._StateProcess(delta);

            _character.AnimationTree.UpdateBlendPosition("Run", _character.MoveComponent.Direction);
        }

        [StateMethod]
        public override void _StateProcessPhysics(double delta)
        {
            base._StateProcessPhysics(delta);

            if (_character.IsNavFinished())
            {
                FSM.RequestState<IdleState>();

                return;
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