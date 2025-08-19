using Game.Core.Serialize;
using Game.Logic.AI.Attacker.Simple.FSM;
using Game.Logic.Components;
using Game.Logic.Interactive;
using Game.Utils;
using Godot;

namespace Game.Logic.AI.Attacker.Simple
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

            if (CanMoveAndSlide)
            {
                if (!PoisonedComponent.IsPoisoned && IsNavFinished())
                    NavigationAgent.TargetPosition =
                        NavigationServer2D.MapGetRandomPoint(GetWorld2D().GetNavigationMap(), 1, false);

                if (!PoisonedComponent.IsPoisoned)
                {
                    if (IsNavFinished())
                    {
                        NavigationAgent.TargetPosition =
                            NavigationServer2D.MapGetRandomPoint(GetWorld2D().GetNavigationMap(), 1, false);
                    }
                }
                else
                {
                    NavigationAgent.TargetPosition = Player.Character.Instance.GlobalPosition;
                }
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

            FSM.RequestState<RunState>();

            NavigationAgent.TargetPosition =
                NavigationServer2D.MapGetRandomPoint(GetWorld2D().GetNavigationMap(), 1, false);
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
        [ExportCategory("Parameters")]
        [ExportGroup("FSM")]
        [ExportSubgroup("AttackState")]
        [Export]
        public float PreAttackWaitTime = 0.2f;
        [Export]
        public float AnimationAttackWaitTime = 0.45f;
        [Export]
        public float PostAttackWaitTime = 0.4f;
        [Export]
        public float AttackDamageValue = 5.0f;
        [ExportSubgroup("RunState")]
        [Export]
        public float RunSpeed = 350.0f;
        [Export]
        public bool CanSpawnDirtOnRun = true;
        [Export]
        public Vector2 SpawnDirtTimerOffsetTime = new(-3.5f, 3.5f);
        [ExportSubgroup("WalkState")]
        [Export]
        public float WalkSpeed = 200.0f;
        #endregion
    }
}