using Game.Core.FSM;
using Game.Utils;
using Godot;

namespace Game.Logic.AI.Attacker.Aimer.FSM
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

            if (_character.IsNavFinished())
            {
                if (_character.PoisonedComponent.IsPoisoned && _character.CanAttack)
                {
                    if (_character.PoisonedComponent.IsPoisoned && _character.CanAttack && 
                        _character.DistanceToPlayer() < _character.MaxDistanceToAttackPlayer)
                    {
                        _character.CanAttack = false;
                
                        FSM.RequestState<AttackState>();

                        return;
                    }

                    _character.NavigationAgent.TargetPosition = _character.NavigationRegion.GetRandomPointRadius(Player.Character.Instance
                        .GlobalPosition, _character.AttackRadius.X, _character.AttackRadius.Y);
                }
            }

            _character.MoveComponent.Move(_character.WalkSpeed);
        }
        #endregion

        #region PRIVATE FIELDS
        private Character _character;
        #endregion
    }
}