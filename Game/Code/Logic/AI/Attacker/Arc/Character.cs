using Game.Core.Logger;
using Game.Core.Serialize;
using Game.Logic.AI.Attacker.Arc.FSM;
using Game.Logic.Components;
using Game.Logic.Interactive;
using Game.Utils;
using Godot;

namespace Game.Logic.AI.Attacker.Arc
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

            if (IsNavFinished() && !PoisonedComponent.IsPoisoned)
                NavigationAgent.TargetPosition =
                    NavigationServer2D.MapGetRandomPoint(GetWorld2D().GetNavigationMap(), 1, false);

            if (!CanChangeNavigationPosition && PoisonedComponent.IsPoisoned && IsNavFinished() && !FSM.IsCurrentStateEqual<AttackState>()
                && FSM.IsCurrentStateEqual<WalkState>())
            {
                if (this.DistanceToPlayer() < MaxDistanceToAttackPlayer)
                    FSM.RequestState<AttackState>();
                else
                    CanChangeNavigationPosition = true;
            }


            if (CanChangeNavigationPosition && IsNavFinished())
            {
                if (CurrentAttackToPlayerCount < MaxAttackToPlayerCount)
                {
                    NavigationAgent.TargetPosition =
                        NavigationRegion.GetRandomPointRadius(Player.Character.Instance.GlobalPosition, AttackRadius.X, AttackRadius.Y);
                    CurrentAttackToPlayerCount++;

                    FSM.RequestState<WalkState>();
                }
                else
                {
                    if (CurrentChangePositionCount < MaxChangePositionCount)
                    {
                        NavigationAgent.TargetPosition =
                            NavigationServer2D.MapGetRandomPoint(GetWorld2D().GetNavigationMap(), 1, false);
                        CurrentChangePositionCount++;

                        FSM.RequestState<RunState>();
                    }
                    else
                    {
                        CurrentAttackToPlayerCount = 1;
                        CurrentChangePositionCount = 0;

                        if (GlobalPosition.DistanceTo(Player.Character.Instance.GlobalPosition) < MaxDistanceToAttackPlayer)
                            FSM.RequestState<AttackState>();
                        else
                        {
                            NavigationAgent.TargetPosition =
                                NavigationRegion.GetRandomPointRadius(Player.Character.Instance.GlobalPosition, AttackRadius.X, AttackRadius.Y);
                            CurrentAttackToPlayerCount++;

                            FSM.RequestState<WalkState>();
                        }
                    }
                }

                CanChangeNavigationPosition = false;
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

            NavigationAgent.TargetPosition =
                NavigationRegion.GetRandomPointRadius(Player.Character.Instance.GlobalPosition, AttackRadius.X, AttackRadius.Y);
            FSM.RequestState<WalkState>();
        }

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Sockets")]
        [Export]
        public Node2D SnotSocket;

        [ExportCategory("Parameters")]
        [ExportGroup("Character Logic")]
        [Export]
        public int MaxAttackToPlayerCount = 2;
        [Export]
        public int MaxChangePositionCount = 2;
        [Export]
        public float MaxDistanceToAttackPlayer = 1500.0f;
        [Export]
        public Vector2 AttackRadius = new Vector2(1000.0f, 1700.0f);

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
        public float TargetMinimumDistance = 500.0f;
        [Export]
        public float TargetMaximumDistance = 1400.0f;
        [Export]
        public bool ShouldProjectileDestroyOnAI = true;
        [ExportSubgroup("RunState")]
        [Export]
        public float RunSpeed = 450.0f;
        [ExportSubgroup("WalkState")]
        [Export]
        public float WalkSpeed = 200.0f;
        #endregion

        #region PUBLIC FIELDS
        public int CurrentAttackToPlayerCount = 0;
        public int CurrentChangePositionCount = 0;
        public bool CanChangeNavigationPosition = false;
        #endregion
    }
}