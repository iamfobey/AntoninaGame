using Game.Core.Serialize;
using Game.Logic.AI.Attacker.Aimer.FSM;
using Game.Utils;
using Godot;

namespace Game.Logic.AI.Attacker.Aimer
{
    [GameSerializable]
    [CodeComments]
    public partial class Character : Core.AI.Attacker.Character
    {
        #region GAME METHODS
        [GameMethod]
        public override void _ReadyGame()
        {
            base._ReadyGame();

            FSM.RequestState<WalkState>();
            
            this.SyncVariables();
        }

        [GameMethod]
        public override void _PhysicsProcessGame(double delta)
        {
            base._PhysicsProcessGame(delta);

            if (IsNavFinished() && !PoisonedComponent.IsPoisoned)
                NavigationAgent.TargetPosition =
                    NavigationServer2D.MapGetRandomPoint(GetWorld2D().GetNavigationMap(), 1, false);
        }
        #endregion

        public override void _OnRemovePoisonBeforeTimer()
        {
            base._OnRemovePoisonBeforeTimer();

            FSM.RequestState<AlwaysIdleState>();
        }

        public override void _OnRemovePoisonAfterTimer()
        {
            base._OnRemovePoisonAfterTimer();

            NavigationAgent.TargetPosition =
                NavigationServer2D.MapGetRandomPoint(GetWorld2D().GetNavigationMap(), 1, false);
            
            FSM.RequestState<RunState>();
        }

        public override void _OnChangeSelfToPoisonBeforeTimer()
        {
            base._OnChangeSelfToPoisonBeforeTimer();

            FSM.RequestState<AlwaysIdleState>();
        }

        public override void _OnChangeSelfToPoisonAfterTimer()
        {
            base._OnChangeSelfToPoisonAfterTimer();

            FSM.RequestState<RunState>();
        }

        #region EDITOR FIELDS
        [ExportCategory("Parameters")]
        [ExportGroup("Character Logic")]
        [Export]
        public float MaxDistanceToAttackPlayer = 1500.0f;
        [Export]
        public Vector2 AttackRadius = new Vector2(500.0f, 1000.0f);
        
        [ExportGroup("FSM")]
        [ExportSubgroup("AttackState")]
        [Export]
        public float PreAttackWaitTime = 0.2f;
        [Export]
        public float AnimationAttackWaitTime = 0.37f;
        [Export]
        public float PostAttackWaitTime = 0.4f;
        [Export]
        public float AttackDamageValue = 5.0f;
        [Export]
        public float AimSpeed = 100.0f;
        [ExportSubgroup("RunState")]
        [Export]
        public float RunSpeed = 800.0f;
        [ExportSubgroup("WalkState")]
        [Export]
        public float WalkSpeed = 550.0f;
        #endregion

        #region PUBLIC FIELDS
        public bool CanAttack = true;
        #endregion
    }
}