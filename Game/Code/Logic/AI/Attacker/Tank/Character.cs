using Game.Core.Serialize;
using Game.Logic.AI.Attacker.Tank.FSM;
using Game.Utils;
using Godot;

namespace Game.Logic.AI.Attacker.Tank
{
    [GameSerializable]
    public partial class Character : Core.AI.Attacker.Character
    {
        #region GAME METHODS
        [GameMethod]
        public override void _ReadyGame()
        {
            base._ReadyGame();

            FSM.RequestState<WalkState>();
        }

        [GameMethod]
        public override void _PhysicsProcessGame(double delta)
        {
            base._PhysicsProcessGame(delta);
        }
        #endregion

        #region PUBLIC METHODS
        public override void _OnRemovePoisonBeforeTimer()
        {
            base._OnRemovePoisonBeforeTimer();

            FSM.RequestState<AlwaysIdleState>();
        }

        public override void _OnRemovePoisonAfterTimer()
        {
            base._OnRemovePoisonAfterTimer();

            FSM.RequestState<RunState>();

            NavigationAgent.TargetPosition =
                NavigationServer2D.MapGetRandomPoint(GetWorld2D().GetNavigationMap(), 1, false);
        }

        public override void _OnChangeSelfToPoisonBeforeTimer()
        {
            base._OnChangeSelfToPoisonBeforeTimer();

            FSM.RequestState<AlwaysIdleState>();
        }

        public override void _OnChangeSelfToPoisonAfterTimer()
        {
            base._OnChangeSelfToPoisonAfterTimer();

            NavigationAgent.TargetPosition = Player.Character.Instance.GlobalPosition;

            FSM.RequestState<AttackState>();
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Parameters")]
        [ExportGroup("Character Logic")]
        [Export]
        public int MaxAttackToPlayerCount = 3;
        [Export]
        public int MaxChangePositionCount = 1;

        [ExportGroup("FSM")]
        [ExportSubgroup("AttackState")]
        [Export]
        public float AttackDamageValue = 5.0f;
        [Export]
        public float AttackWalkSpeed = 850.0f;
        [Export]
        public int MaxAttackCount = 3;
        [Export]
        public int MaxIdleCount = 1;
        [ExportSubgroup("AvoidWalkHitState")]
        [Export]
        public float AvoidWalkHitSpeed = 1300.0f;
        [ExportSubgroup("AvoidWalkState")]
        [Export]
        public float AvoidWalkSpeed = 500.0f;
        [Export]
        public Vector2 AvoidWalkToHitRadius = new(2000.0f, 3000.0f);
        [ExportSubgroup("IdleState")]
        [Export]
        public Vector2 AvoidIdleToWalkRadius = new(1000.0f, 2000.0f);
        [ExportSubgroup("RunState")]
        [Export]
        public float RunSpeed = 550.0f;
        [ExportSubgroup("WalkState")]
        [Export]
        public float WalkSpeed = 200.0f;
        #endregion

        #region PUBLIC FIELDS
        public int CurrentAttackToPlayerCount = 0;
        public int CurrentChangePositionCount = 0;
        #endregion
    }
}