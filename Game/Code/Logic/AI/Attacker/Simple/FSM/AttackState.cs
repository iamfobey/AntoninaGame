using System;
using Game.Core.FSM;
using Game.Utils;
using Godot;

namespace Game.Logic.AI.Attacker.Simple.FSM
{
    [Tool]
    public partial class AttackState : State
    {
        #region ENUMS
        public enum EAttackWaitType
        {
            PreWait,
            AnimationWait,
            PostWait
        }
        #endregion

        #region FSM STATE METHODS
        [StateMethod]
        public override void _StateReady()
        {
            base._StateReady();

            _character = Root<Character>();

            _globalSubscriptions.Add(AttackTimer, Timer.SignalName.Timeout, Callable.From(_OnAttackTimerTimeout));

            _globalSubscriptions.Add(AttackArea, Area2D.SignalName.BodyEntered, Callable.From<Node2D>(_OnAttackAreaBodyEntered));
            _globalSubscriptions.Add(AttackArea, Area2D.SignalName.BodyExited, Callable.From<Node2D>(_OnAttackAreaBodyExited));
        }

        [StateMethod]
        public override void _StateEnter()
        {
            base._StateEnter();

            _character.SpineAnimationManager.SetAnimationByDirection("Idle", _character.MoveComponent.Direction);

            AttackWaitType = EAttackWaitType.PreWait;
            AttackTimer.Start(_character.PreAttackWaitTime);


            _character.MoveComponent.Speed = 0.0f;
        }

        [StateMethod]
        public override void _StateExit()
        {
            base._StateExit();

            AttackTimer.Stop();
        }
        #endregion

        #region SIGNAL METHODS
        [SignalMethod]
        public void _OnAttackTimerTimeout()
        {
            switch (AttackWaitType)
            {
                case EAttackWaitType.PreWait:
                    if (AttackedPlayerCharacter != null)
                    {
                        _character.SpineAnimationManager.SetAnimation("Attack");

                        AttackWaitType = EAttackWaitType.AnimationWait;
                        AttackTimer.Start(_character.AnimationAttackWaitTime);
                    }
                    else
                    {
                        FSM.RequestState<RunState>();
                    }
                    break;
                case EAttackWaitType.AnimationWait:
                    AttackedPlayerCharacter?.HealthComponent.ApplyDamage(_character, _character.AttackDamageValue);

                    AttackWaitType = EAttackWaitType.PostWait;
                    AttackTimer.Start(_character.PostAttackWaitTime);
                    break;
                case EAttackWaitType.PostWait:
                    if (AttackedPlayerCharacter != null)
                    {
                        AttackWaitType = EAttackWaitType.PreWait;
                        AttackTimer.Start(_character.PreAttackWaitTime);
                    }
                    else
                    {
                        FSM.RequestState<RunState>();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [SignalMethod]
        public void _OnAttackAreaBodyEntered(Node2D body)
        {
            if (_character.PoisonedComponent.IsPoisoned && body is AttackBody { Root: Player.Character playerCharacter, Type: "Middle" })
            {
                AttackedPlayerCharacter = playerCharacter;
                FSM.RequestState<AttackState>();
            }
        }

        [SignalMethod]
        public void _OnAttackAreaBodyExited(Node2D body)
        {
            if (body is AttackBody { Root: Player.Character, Type: "All" })
            {
                AttackedPlayerCharacter = null;
            }
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public Timer AttackTimer;
        [Export]
        public Area2D AttackArea;
        #endregion

        #region PUBLIC FIELDS
        public Player.Character AttackedPlayerCharacter { get; private set; }

        public EAttackWaitType AttackWaitType = EAttackWaitType.PreWait;
        #endregion

        #region PRIVATE FIELDS
        private Character _character;
        #endregion
    }
}