using Game.Core.FSM;
using Game.Utils;
using Godot;

namespace Game.Logic.AI.Attacker.Sniper.FSM
{
    [Tool]
    public partial class WalkState : State
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

            _character.AnimationTree.RequestAnimation("Transition", "Walk");
        }

        [StateMethod]
        public override void _StateProcess(double delta)
        {
            base._StateProcess(delta);

            _character.AnimationTree.UpdateBlendPosition("Walk", _character.MoveComponent.Direction);
        }

        [StateMethod]
        public override void _StateProcessPhysics(double delta)
        {
            base._StateProcessPhysics(delta);

            _character.MoveComponent.Move(_character.WalkSpeed);
        }
        #endregion

        #region PRIVATE FIELDS
        private Character _character;
        #endregion
    }
}