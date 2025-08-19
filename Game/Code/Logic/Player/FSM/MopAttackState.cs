using System;
using Game.Core.FSM;
using Game.Core.Input;
using Game.Core.Logger;
using Game.Logic.Interactive;
using Game.Utils;
using Godot;
using Server = Game.Core.Input.Server;

namespace Game.Logic.Player.FSM
{
    [Tool]
    public partial class MopAttackState : State
    {
        #region ENUMS
        public enum EType
        {
            Mop,
            Bucket
        }
        #endregion

        #region FSM STATE METHODS
        [StateMethod]
        public override void _StateReady()
        {
            base._StateReady();

            _character = Root<Character>();

            _timer = this.CreateAndAddChild(new SpineAnimationTimer(Callable.From(_OnAttackAnimationCompleted)));

            _globalSubscriptions.Add(ComboResetTimer, Timer.SignalName.Timeout, Callable.From(_OnComboResetTimerTimeout));
        }

        [StateMethod]
        public override void _StateEnter()
        {
            base._StateEnter();
            
            _localSubsribtions.Add(HitDelayTimer, Timer.SignalName.Timeout, Callable.From(_OnHitDelayTimerTimeout));

            ComboResetTimer.Stop();

            ComboStep++;
            if (ComboStep > MaxComboSteps) ComboStep = 1;

            AttackArea.SetActive(true, true);
            AttackArea.Monitoring = true;

            Type = ComboStep switch
            {
                1 => EType.Mop, 2 => EType.Mop, 3 => EType.Bucket,
                _ => throw new ArgumentOutOfRangeException()
            };

            _character.SpineAnimationManager.SetAnimationByDirection("Attack", _character.PrevMoveComponent.Direction);
            _character.CanUpdateInput = false;
            _character.MoveComponent.Speed = 0.0f;

            HitDelayTimer.Start(0.25f);

            _timer.Start(_character.SpineAnimationManager);
        }

        [StateMethod]
        public override void _StateProcessPhysics(double delta)
        {
            base._StateProcessPhysics(delta);

            if (Server.IsActionJustPressed(Map.PlayerRun))
            {
                FSM.RequestState<DashState>();
                return;
            }

            if (Server.IsActionJustPressed(Map.PlayerBlock))
            {
                FSM.RequestState<BlockState>();
            }
        }

        [StateMethod]
        public override void _StateExit()
        {
            base._StateExit();

            HitDelayTimer.Stop();

            _timer.Stop();

            AttackArea.SetActive(true, false);
            AttackArea.Monitoring = false;
        }
        #endregion

        #region SIGNAL METHODS
        [SignalMethod]
        public void _OnAttackAnimationCompleted()
        {
            if (ComboStep == MaxComboSteps)
            {
                ComboStep = 0;
                FSM.RequestState<IdleState>();
            }
            else
            {
                ComboResetTimer.Start();
                FSM.RequestState<IdleState>();
            }

            _character.CanUpdateInput = true;
            _character.Velocity = Vector2.Zero;
        }

        [SignalMethod]
        public void _OnComboResetTimerTimeout()
        {
            ComboStep = 0;
        }

        [SignalMethod]
        private void _OnHitDelayTimerTimeout()
        {
            var overlappingBodies = AttackArea.GetOverlappingBodies();
            foreach (var body in overlappingBodies)
            {
                if (body is AttackBody { Root: Barrel barrel, Type: "All" })
                {
                    barrel.Explode();
                    continue;
                }
                
                if (body is AttackBody { Root: Core.AI.Character aiCharacter, Type: "All" })
                {
                    var direction = (aiCharacter.GlobalPosition - _character.GlobalPosition).Normalized();

                    if (direction.IsZeroApprox())
                    {
                        direction = _character.PrevMoveComponent.Direction;
                    }

                    aiCharacter.HealthComponent.ApplyDamage(_character, _character.MopAttackDamageValue);
                    aiCharacter.ApplyHitReaction(direction, ComboStep == 3 ? 1200.0f : 0.0f, 0.0f, 0.35f);
                }
            }
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public Timer ComboResetTimer;
        [Export]
        public Timer HitDelayTimer;
        [Export]
        public Area2D AttackArea;

        [ExportCategory("Parameters")]
        [ExportGroup("Logic")]
        [Export]
        public int MaxComboSteps = 3;
        #endregion

        #region PUBLIC FIELDS
        public EType Type;
        public int ComboStep = 0;
        #endregion

        #region PRIVATE FIELDS
        private SpineAnimationTimer _timer;
        private Character _character;
        #endregion
    }
}