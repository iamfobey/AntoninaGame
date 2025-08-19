using Game.Core.FSM;
using Game.Utils;
using Godot;

namespace Game.Logic.AI.Attacker.Tank.FSM
{
    [Tool]
    public partial class AttackIdleState : State
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

            _character.NavigationAgent.TargetPosition = _character.GlobalPosition;

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
            if (_character.PoisonedComponent.IsPoisoned && FSM.GetState<AttackState>().CurrentPlayerCharacter != null)
            {
                _character.NavigationAgent.TargetPosition =
                    _character.NavigationRegion.GetRandomPointRadius(_character.GlobalPosition, 500.0f, 2000.0f);

                FSM.RequestState<AvoidWalkState>();

                return;
            }

            if (_character.PoisonedComponent.IsPoisoned)
            {
                _character.NavigationAgent.TargetPosition = Player.Character.Instance.GlobalPosition;

                FSM.RequestState<AttackState>();
            }
            else
            {
                _character.NavigationAgent.TargetPosition =
                    _character.NavigationRegion.GetRandomPointRadius(_character.GlobalPosition, 500.0f, 2000.0f);

                FSM.RequestState<WalkState>();
            }
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