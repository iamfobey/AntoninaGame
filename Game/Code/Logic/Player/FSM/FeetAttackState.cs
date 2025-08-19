using Game.Core.FSM;
using Game.Logic.Interactive;
using Game.Utils;
using Godot;

namespace Game.Logic.Player.FSM
{
    [Tool]
    public partial class FeetAttackState : State
    {
        #region FSM STATE METHODS
        [StateMethod]
        public override void _StateReady()
        {
            base._StateReady();

            _character = Root<Character>();

            _timer = this.CreateAndAddChild(new SpineAnimationTimer(Callable.From(_OnAttackAnimationCompleted)));
        }

        [StateMethod]
        public override void _StateEnter()
        {
            base._StateEnter();
            
            _localSubsribtions.Add(HitDelayTimer, Timer.SignalName.Timeout, Callable.From(_OnHitDelayTimerTimeout));

            AttackArea.SetActive(true, true);
            AttackArea.Monitoring = true;

            _character.SpineAnimationManager.SetAnimationByDirection("Attack", _character.PrevMoveComponent.Direction);
            _character.CanUpdateInput = false;
            _character.MoveComponent.Speed = 0.0f;
            
            HitDelayTimer.Start(0.25f);

            _timer.Start(_character.SpineAnimationManager);
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
            FSM.RequestState<IdleState>();

            _character.CanUpdateInput = true;
            _character.Velocity = Vector2.Zero;
        }

        [SignalMethod]
        private void _OnHitDelayTimerTimeout()
        {
            var overlappingBodies = AttackArea.GetOverlappingBodies();
            foreach (var body in overlappingBodies)
            {
                if (body is AttackBody { Root: Barrel barrel, Type: "All" })
                {
                    var direction = (barrel.GlobalPosition - _character.GlobalPosition).Normalized();
                    if (direction.IsZeroApprox())
                    {
                        direction = _character.PrevMoveComponent.Direction;
                    }
                    barrel.HandleKick(direction, 1000.0f);
                    continue;
                }
                
                if (body is AttackBody { Root: Core.AI.Character aiCharacter, Type: "All" })
                {
                    var direction = (aiCharacter.GlobalPosition - _character.GlobalPosition).Normalized();

                    if (direction.IsZeroApprox())
                    {
                        direction = _character.PrevMoveComponent.Direction;
                    }

                    aiCharacter.HealthComponent.ApplyDamage(_character, _character.FeetAttackDamageValue);
                    aiCharacter.ApplyHitReaction(direction, 5000.0f, 0.0f, 0.3f);
                }
            }
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public Area2D AttackArea;
        [Export]
        public Timer HitDelayTimer;
        #endregion

        #region PRIVATE FIELDS
        private SpineAnimationTimer _timer;
        private Character _character;
        #endregion
    }
}