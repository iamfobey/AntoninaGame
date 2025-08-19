using Game.Core.FSM;
using Game.Core.Logger;
using Game.Logic.Components;
using Game.Utils;
using Godot;
using Godot.Collections;

namespace Game.Logic.AI.Attacker.Tank.FSM
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

            _globalSubscriptions.Add(_character.HealthComponent, HealthComponent.SignalName.OnApplyDamage,
                Callable.From<Node, float, Dictionary>(_OnHealthComponentApplyDamage));
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
            if (_character.PoisonedComponent.IsPoisoned && FSM.GetState<AttackState>().CurrentPlayerCharacter != null)
            {
                var attackState = FSM.GetState<AttackState>();
                attackState.CurrentAttackCount = 0;
                attackState.CurrentIdleCount = 0;

                _character.NavigationAgent.TargetPosition =
                    _character.NavigationRegion.GetRandomPointRadius(_character.GlobalPosition, _character.AvoidIdleToWalkRadius.X,
                        _character
                            .AvoidIdleToWalkRadius.Y);

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

        [SignalMethod]
        public void _OnHealthComponentApplyDamage(Node from, float value, Dictionary parameters)
        {
            if (FSM.IsCurrentStateEqual<AvoidWalkHitState>() || !_character.PoisonedComponent.IsPoisoned)
                return;

            _character.NavigationAgent.TargetPosition =
                _character.NavigationRegion.GetRandomPointRadius(_character.GlobalPosition, _character.AvoidWalkToHitRadius.X,
                    _character.AvoidWalkToHitRadius.Y);

            FSM.RequestState<AvoidWalkHitState>();
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