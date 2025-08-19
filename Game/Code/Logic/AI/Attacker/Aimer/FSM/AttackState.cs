using System;
using Game.Core.FSM;
using Game.Logic.Components;
using Game.Logic.Misc;
using Game.Utils;
using Godot;
using Godot.Collections;

namespace Game.Logic.AI.Attacker.Aimer.FSM
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
            _globalSubscriptions.Add(_character.HealthComponent, HealthComponent.SignalName.OnApplyDamage,
                Callable.From<Node, float, Dictionary>(_OnHealthComponentApplyDamage));
        }

        [StateMethod]
        public override void _StateEnter()
        {
            base._StateEnter();

            _character.MoveComponent.Speed = 0.0f;

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
                    _character.AnimationTree.RequestAnimation("Transition", "Attack")
                        .UpdateBlendPosition("Attack", _character.MoveComponent.Direction);

                    AttackWaitType = EAttackWaitType.AnimationWait;
                    AttackTimer.Start(_character.AnimationAttackWaitTime);
                    break;
                case EAttackWaitType.AnimationWait:
                    AttackWaitType = EAttackWaitType.PostWait;
                    AttackTimer.Start(_character.PostAttackWaitTime);
                    break;
                case EAttackWaitType.PostWait:
                    var projectile = (AimProjectile)Projectile.Instantiate();
                    projectile.GlobalPosition = Player.Character.Instance.GlobalPosition;
                    projectile.Speed = _character.AimSpeed;
                    projectile.OnLifeTimeout += self =>
                    {
                        var bodies = ((AimProjectile)self).Area.GetOverlappingBodies();

                        foreach (var body in bodies)
                        {
                            if (body is AttackBody { Root: Player.Character playerCharacter, Type: "All" })
                            {
                                playerCharacter.CanUpdateInput = false;
                                playerCharacter.MoveComponent.ApplyStun(this, 3.5f, () =>
                                {
                                    playerCharacter.CanUpdateInput = true;
                                });
                                playerCharacter.HealthComponent.ApplyDamage(this, _character.AttackDamageValue);
                            }
                        }
                        self.QueueFree();

                        FSM.RequestState<IdleState>();
                    };
                    projectile.Initialize();
                    GetTree().Root.AddChild(projectile, true);
                    _currentProjectile = projectile;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [SignalMethod]
        public void _OnHealthComponentApplyDamage(Node from, float value, Dictionary parameters)
        {
            if (!FSM.IsCurrentStateEqual<AttackState>()) return;

            if (from is Player.Character)
            {
                if (IsInstanceValid(_currentProjectile) && !_currentProjectile.IsQueuedForDeletion())
                {
                    _currentProjectile.QueueFree();
                }

                FSM.RequestState<AvoidAttackState>();
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
        #endregion

        #region PUBLIC FIELDS
        public EAttackWaitType AttackWaitType = EAttackWaitType.PreWait;
        #endregion

        #region PRIVATE FIELDS
        private Character _character;
        private Projectile _currentProjectile;
        #endregion
    }
}