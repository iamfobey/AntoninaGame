using Game.Core.FSM;
using Game.Logic.Interactive;
using Game.Utils;
using Godot;

namespace Game.Logic.AI.Attacker.Simple.FSM
{
    [Tool]
    public partial class RunState : State
    {
        #region FSM STATE METHODS
        [StateMethod]
        public override void _StateReady()
        {
            base._StateReady();

            _character = Root<Character>();

            _globalSubscriptions.Add(SpawnDirtTimer, Timer.SignalName.Timeout, Callable.From(_OnSpawnDirtTimerTimeout));
        }

        [StateMethod]
        public override void _StateEnter()
        {
            base._StateEnter();

            SpawnDirtTimer.Start();
        }

        [StateMethod]
        public override void _StateProcess(double delta)
        {
            base._StateProcess(delta);

            _character.SpineAnimationManager.SetAnimationByDirection("Run", _character.MoveComponent.Direction);
        }

        [StateMethod]
        public override void _StateProcessPhysics(double delta)
        {
            base._StateProcessPhysics(delta);

            _character.MoveComponent.Move(_character.RunSpeed);
        }

        [StateMethod]
        public override void _StateExit()
        {
            base._StateExit();

            SpawnDirtTimer.Stop();
        }
        #endregion

        #region SIGNAL METHODS
        [SignalMethod]
        public void _OnSpawnDirtTimerTimeout()
        {
            if (_character.CanSpawnDirtOnRun)
            {
                var dirt = DirtPrefab.Instantiate<Dirt>();
                dirt.Type = _character.PoisonedComponent.IsPoisoned ? Dirt.EType.Poison : Dirt.EType.Default;
                dirt.GlobalPosition = _character.GlobalPosition;
                GetNode<Node2D>("/root/Root/Interactive").AddChild(dirt);

                _spawnedDirtCount++;
            }
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public Timer SpawnDirtTimer;
        [Export]
        public PackedScene DirtPrefab;
        #endregion

        #region PRIVATE FIELDS
        private Character _character;
        private int _spawnedDirtCount;
        #endregion
    }
}