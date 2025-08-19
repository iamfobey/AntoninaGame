using Game.Core.FSM;
using Game.Utils;
using Godot;

namespace Game.Logic.AI.Attacker.Aimer.FSM
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

            _globalSubscriptions.Add(IdleTimer, Timer.SignalName.Timeout, Callable.From(_OnIdleTimerTimeout));
        }

        [StateMethod]
        public override void _StateEnter()
        {
            base._StateEnter();

            _character.AnimationTree.RequestAnimation("Transition", "Idle")
                .UpdateBlendPosition("Idle", _character.MoveComponent.Direction);

            _character.MoveComponent.Speed = 0.0f;

            IdleTimer.Start();
        }

        [StateMethod]
        public override void _StateExit()
        {
            base._StateExit();

            IdleTimer.Stop();
        }
        #endregion

        #region SIGNAL METHODS
        [SignalMethod]
        public void _OnIdleTimerTimeout()
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

            _character.CanAttack = true;

            FSM.RequestState<WalkState>();
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public Timer IdleTimer;
        #endregion

        #region PRIVATE FIELDS
        private Character _character;
        #endregion
    }
}