using Game.Core.FSM;
using Game.Logic.Components;
using Game.Utils;
using Godot;
using Godot.Collections;

namespace Game.Logic.AI.Attacker.Tank.FSM
{
    [Tool]
    public partial class AvoidWalkState : State
    {
        #region FSM STATE METHODS
        [StateMethod]
        public override void _StateReady()
        {
            base._StateReady();

            _character = Root<Character>();

            _globalSubscriptions.Add(_character.HealthComponent, HealthComponent.SignalName.OnApplyDamage,
                Callable.From<Node, float, Dictionary>(_OnHealthComponentApplyDamage));
        }

        [StateMethod]
        public override void _StateEnter()
        {
            base._StateEnter();

            _character.AnimationTree.RequestAnimation("Transition", "Walk")
                .UpdateBlendPosition("Walk", _character.MoveComponent.Direction);
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

            if (_character.IsNavFinished())
            {
                FSM.RequestState<AvoidIdleState>();

                return;
            }

            _character.MoveComponent.Move(_character.AvoidWalkSpeed);
        }
        #endregion

        #region SIGNAL METHODS
        [SignalMethod]
        public void _OnHealthComponentApplyDamage(Node from, float value, Dictionary parameters)
        {
            if (!FSM.IsCurrentStateEqual<AvoidWalkState>() || !_character.PoisonedComponent.IsPoisoned)
                return;

            _character.NavigationAgent.TargetPosition =
                _character.NavigationRegion.GetRandomPointRadius(_character.GlobalPosition, _character.AvoidWalkToHitRadius.X,
                    _character.AvoidWalkToHitRadius.Y);
            FSM.RequestState<AvoidWalkHitState>();
        }
        #endregion

        #region PRIVATE FIELDS
        private Character _character;
        #endregion
    }
}