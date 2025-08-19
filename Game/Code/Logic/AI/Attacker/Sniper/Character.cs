using Game.Core.Serialize;
using Game.Logic.AI.Attacker.Sniper.FSM;
using Game.Utils;
using Godot;

namespace Game.Logic.AI.Attacker.Sniper
{
    [GameSerializable]
    public partial class Character : Core.AI.Attacker.Character
    {
        #region GAME METHODS
        [GameMethod]
        public override void _ReadyGame()
        {
            base._ReadyGame();

            FSM.RequestState<RunState>();
        }

        [GameMethod]
        public override void _PhysicsProcessGame(double delta)
        {
            base._PhysicsProcessGame(delta);

            if (IsNavFinished() && !PoisonedComponent.IsPoisoned)
                NavigationAgent.TargetPosition =
                    NavigationServer2D.MapGetRandomPoint(GetWorld2D().GetNavigationMap(), 1, false);

            if (IsNavFinished() && PoisonedComponent.IsPoisoned && !FSM.IsCurrentStateEqual<AttackState>())
            {
                if (this.DistanceToPlayer() < MaxDistanceToAttackPlayer)
                    FSM.RequestState<AttackState>();
                else
                    NavigationAgent.TargetPosition = 
                        NavigationRegion.GetRandomPointRadius(Player.Character.Instance.GlobalPosition, AttackRadius.X, AttackRadius.Y);
            }

            if (CanChangeNavigationPosition && IsNavFinished())
            {
                CanChangeNavigationPosition = false;
                
                NavigationAgent.TargetPosition = 
                    NavigationRegion.GetRandomPointRadius(Player.Character.Instance.GlobalPosition, AttackRadius.X, AttackRadius.Y);

                FSM.RequestState<RunState>();
            }
        }
        #endregion

        public override void _OnRemovePoisonBeforeTimer()
        {
            base._OnRemovePoisonBeforeTimer();

            FSM.RequestState<IdleState>();
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

            FSM.RequestState<IdleState>();
        }

        public override void _OnChangeSelfToPoisonAfterTimer()
        {
            base._OnChangeSelfToPoisonAfterTimer();

            FSM.RequestState<RunState>();
        }

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Sockets")]
        [Export]
        public Node2D LaserSocket;

        [ExportCategory("Parameters")]
        [ExportGroup("Character Logic")]
        [Export]
        public float MaxDistanceToAttackPlayer = 2000.0f;
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
        public bool ShouldProjectileDestroyOnAI = true;
        [ExportSubgroup("RunState")]
        [Export]
        public float RunSpeed = 500.0f;
        [ExportSubgroup("WalkState")]
        [Export]
        public float WalkSpeed = 300.0f;
        #endregion

        #region PUBLIC FIELDS
        public bool CanChangeNavigationPosition = false;
        #endregion
    }
}