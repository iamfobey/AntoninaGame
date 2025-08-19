using System;
using Game.Core.FSM;
using Game.Logic.Misc;
using Game.Utils;
using Godot;

namespace Game.Logic.AI.Attacker.Arc.FSM
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
        }

        [StateMethod]
        public override void _StateEnter()
        {
            base._StateEnter();

            _character.MoveComponent.Speed = 0.0f;

            _character.SpineAnimationManager.SetAnimationByDirection("Idle", _character.MoveComponent.Direction);

            AttackWaitType = EAttackWaitType.PreWait;
            AttackTimer.Start(_character.PreAttackWaitTime);
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
                    _character.SpineAnimationManager.SetAnimation("Attack");

                    AttackWaitType = EAttackWaitType.AnimationWait;
                    AttackTimer.Start(_character.AnimationAttackWaitTime);
                    break;
                case EAttackWaitType.AnimationWait:
                    AttackWaitType = EAttackWaitType.PostWait;
                    AttackTimer.Start(_character.PostAttackWaitTime);
                    break;
                case EAttackWaitType.PostWait:
                    var projectile = (SnotProjectile)Projectile.Instantiate();
                    projectile.GlobalPosition = _character.SnotSocket.GlobalPosition;
                    projectile.TargetPosition = Player.Character.Instance.MiddleSocket.GlobalPosition;
                    projectile.Speed = 1000.0f;
                    projectile.OnAreaBodyEntered += _OnProjectileAreaBodyEntered;
                    GetTree().Root.AddChild(projectile, true);

                    _character.CanChangeNavigationPosition = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [SignalMethod]
        public void _OnProjectileAreaBodyEntered(Projectile self, Node2D body)
        {
            switch (body)
            {
                case AttackBody { Root: Player.Character playerCharacter, Type: "All" }:
                    playerCharacter.HealthComponent.ApplyDamage(_character, _character.AttackDamageValue);
                    self.QueueFree();
                    break;
                case AttackBody { Root: Core.AI.Character aiCharacter, Type: "All" }:
                    if (aiCharacter != _character && _character.ShouldProjectileDestroyOnAI)
                        self.QueueFree();
                    break;
            }
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public Timer AttackTimer;
        [Export]
        public PackedScene Projectile;
        [Export]
        public PackedScene DirtPrefab;
        #endregion

        #region PUBLIC FIELDS
        public EAttackWaitType AttackWaitType = EAttackWaitType.PreWait;
        #endregion

        #region PRIVATE FIELDS
        private Character _character;
        #endregion
    }
}