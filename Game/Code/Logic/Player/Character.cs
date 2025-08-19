using System;
using Game.Core.Input;
using Game.Core.Serialize;
using Game.Logic.Components;
using Game.Logic.Interactive;
using Game.Logic.Player.FSM;
using Game.Utils;
using Godot;
using Godot.Collections;
using Server = Game.Core.Input.Server;

namespace Game.Logic.Player
{
    [GameSerializable]
    public partial class Character : DisposableCharacterBody2D
    {
        #region GAME METHODS
        [GameMethod]
        public override void _EnterTreeGame()
        {
            base._EnterTreeGame();

            Instance = this;

            this.SyncVariables();
        }

        [GameMethod]
        public override void _ReadyGame()
        {
            base._ReadyGame();

            _subscriptions.Add(MoveComponent, MoveComponent.SignalName.OnApplyStun,
                Callable.From<Node, float, Dictionary>(_OnMoveComponentApplyStun));
            _subscriptions.Add(MoveComponent, MoveComponent.SignalName.OnStunTimeout, Callable.From(_OnMoveComponentStunTimeout));

            _subscriptions.Add(HealthComponent, HealthComponent.SignalName.OnApplyDamage,
                Callable.From<Node, float, Dictionary>(DamageEffectComponent._OnHealthComponentApplyDamage));

            HealthComponent.ApplyHeal(this, MaxHealth);
            StaminaComponent.RegenerateStamina(this, MaxStamina);

            FSM.RequestState<IdleState>();

            _directionBufferTimer = this.CreateAndAddChild(new Timer());
            _directionBufferTimer.WaitTime = 0.1f;
            _directionBufferTimer.OneShot = false;
            _directionBufferTimer.Autostart = false;
        }

        [GameMethod]
        public override void _PhysicsProcessGame(double delta)
        {
            base._PhysicsProcessGame(delta);

            Velocity = MoveComponent.Velocity;

            MoveAndSlide();
        }

        [GameMethod]
        public override void _ProcessGame(double delta)
        {
            base._ProcessGame(delta);

            if (CanUpdateInput)
            {
                MoveComponent.Direction = Server
                    .GetVector(Map.PlayerMoveLeft, Map.PlayerMoveRight, Map.PlayerMoveForward, Map.PlayerMoveBackward, ignoreImGui: true)
                    .Normalized();

                if (MoveComponent.Direction != Vector2.Zero)
                {
                    if (!_directionBufferTimer.IsStopped())
                        _directionBufferTimer.Stop();

                    if (Server.IsActionJustPressed(Map.PlayerRun, true) && StaminaComponent.HasEnoughStamina(DashStamina))
                    {
                        FSM.RequestState<DashState>();
                    }
                    else if (!FSM.IsCurrentStateEqual<MopDashState>() && !FSM.IsCurrentStateEqual<RagAttackState>())
                    {
                        FSM.RequestState<WalkState>();
                    }

                    PrevMoveComponent.Direction = MoveComponent.Direction;
                    PrevMoveComponent.Velocity = MoveComponent.Velocity;
                }
                else
                {
                    if (!IgnoreDirectionBuffer)
                    {
                        FSM.RequestState<IdleState>();
                    }
                    else
                    {
                        if (_directionBufferTimer.IsStopped())
                            _directionBufferTimer.Start();

                        if (!_directionBufferTimer.IsStopped() && _directionBufferTimer.TimeLeft <= 0)
                            FSM.RequestState<IdleState>();
                    }
                }

                if (Server.IsActionJustPressed(Map.PlayerBlock))
                {
                    FSM.RequestState<BlockState>();
                }

                if (Server.IsActionJustPressed(Map.PlayerMopAttack))
                {
                    FSM.RequestState<MopAttackState>();
                }

                if (Server.IsActionPressed(Map.PlayerRagAttack))
                {
                    FSM.RequestState<RagAttackState>();
                }
                
                if (Server.IsActionJustPressed(Map.PlayerFeetAttack))
                {
                    FSM.RequestState<FeetAttackState>();
                }
                
                if (Camera.Offset != Vector2.Zero)
                {
                    Camera.Offset = Camera.Offset.Lerp(Vector2.Zero, (float)delta * CameraReturnSmoothing);

                    if (Camera.Offset.Length() < 0.1f)
                    {
                        Camera.Offset = Vector2.Zero;
                    }
                }
            }

            foreach (var dirt in _cleaningInProgress)
            {
                dirt.Modulate = dirt.Modulate with
                {
                    A = dirt.Modulate.A - 1.0f * (float)delta
                };

                if (dirt.Modulate.A <= 0.15f)
                    dirt.QueueFree();

                switch (dirt.Type)
                {
                    case Dirt.EType.Default:
                        PowerComponent.AddPower(this, 5.0f * (float)delta);
                        break;
                    case Dirt.EType.Poison:
                        PowerComponent.AddPower(this, 10.0f * (float)delta);
                        HealthComponent.ApplyDamage(this, 2.5f * (float)delta, new Dictionary
                            {
                                { "NoDamageEffect", true }
                            }
                        );
                        break;
                    case Dirt.EType.Heal:
                        HealthComponent.ApplyHeal(this, CleanHealValue * (float)delta);
                        break;
                    case Dirt.EType.Chemical:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        #endregion

        #region SIGNAL METHODS
        [SignalMethod]
        public void _OnDirtEntered(Dirt dirt, bool inner)
        {
            if (!inner)
                return;

            if (FSM.IsCurrentStateEqual<MopDashState>())
            {
                _cleaningInProgress.Add(dirt);
            }
        }

        [SignalMethod]
        public void _OnDirtExited(Dirt dirt, bool inner)
        {
            if (!inner)
                return;

            if (FSM.IsCurrentStateEqual<MopDashState>())
            {
                _cleaningInProgress.Remove(dirt);
            }
        }

        [SignalMethod]
        public void _OnMoveComponentApplyStun(Node from, float value, Dictionary parameters)
        {
            FSM.RequestState<IdleState>(null, true);
            SpineAnimationManager.SpineAnimationState.GetCurrent(0).SetTimeScale(0.0f);
        }

        [SignalMethod]
        public void _OnMoveComponentStunTimeout()
        {
            SpineAnimationManager.SpineAnimationState.GetCurrent(0).SetTimeScale(1.0f);
        }
        #endregion

        #region STATIC FIELDS
        public static Character Instance { get; private set; }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public Core.FSM.FSM FSM;
        [Export]
        public SpineAnimationManager SpineAnimationManager;
        [Export]
        public Camera2D Camera;

        [ExportGroup("Sockets")]
        [Export]
        public Node2D HeadSocket;
        [Export]
        public Node2D MiddleSocket;

        [ExportGroup("Components")]
        [Export]
        public HealthComponent HealthComponent;
        [Export]
        public DamageEffectComponent DamageEffectComponent;
        [Export]
        public StaminaComponent StaminaComponent;
        [Export]
        public MoveComponent MoveComponent;
        [Export]
        public PowerComponent PowerComponent;

        [ExportGroup("Debug")]
        [Export]
        public Dictionary<string, NodePath> Teleports = new();

        [ExportCategory("Parameters")]
        [ExportGroup("Logic")]
        [Export]
        public float CameraReturnSmoothing = 2.0f;
        [ExportGroup("FSM")]
        [ExportSubgroup("MopAttackState")]
        [Export]
        public float MopAttackDamageValue = 5.0f;
        [ExportSubgroup("FeetAttackState")]
        [Export]
        public float FeetAttackDamageValue = 5.0f;
        [ExportSubgroup("CleanState")]
        [Export]
        public float CleanHealValue = 5.0f;
        [ExportSubgroup("DashState")]
        [Export]
        public float DashSpeed = 2500.0f;
        [Export]
        public float DashStamina = 10.0f;
        [ExportSubgroup("MopDashState")]
        [Export]
        public float MopDashSpeed = 1200.0f;
        [Export]
        public float MopDashStamina = 1.0f;
        [Export]
        public float DashTimeScale = 1.0f;
        [ExportSubgroup("BlockState")]
        [Export]
        public float BlockTimeScale = 1.2f;
        [ExportSubgroup("WalkState")]
        [Export]
        public float WalkSpeed = 450.0f;
        [ExportSubgroup("RunState")]
        [Export]
        public float RunSpeed = 700.0f;

        [ExportGroup("Components")]
        [ExportSubgroup("HealthComponent")]
        [Export]
        [SyncVar(nameof(HealthComponent))]
        public float MaxHealth = 100.0f;
        [Export]
        [SyncVar(nameof(HealthComponent))]
        public float HealthRegenerationRate = 5.0f;
        [Export]
        [SyncVar(nameof(HealthComponent))]
        public bool CanRegenerateHealth = false;
        [Export]
        [SyncVar(nameof(HealthComponent))]
        public bool CanReceiveDamage = true;
        [Export]
        [SyncVar(nameof(HealthComponent))]
        public bool CanReceiveHeal = true;
        [ExportSubgroup("StaminaComponent")]
        [Export]
        [SyncVar(nameof(StaminaComponent))]
        public float MaxStamina = 100.0f;
        [Export]
        [SyncVar(nameof(StaminaComponent))]
        public float StaminaRegenerationRate = 10.0f;
        [Export]
        [SyncVar(nameof(StaminaComponent))]
        public float StaminaRegenerationRateMultiplayer = 1.005f;
        [Export]
        [SyncVar(nameof(StaminaComponent))]
        public bool CanRegenerateStamina = false;
        #endregion

        #region PUBLIC FIELDS
        public Dirt CurrentDirt { get; private set; }
        public bool CanUpdateInput = true;
        public MoveComponent PrevMoveComponent = new();

        public bool IgnoreDirectionBuffer = false;
        #endregion

        #region PRIVATE FIELDS
        private Array<Dirt> _cleaningInProgress = [];
        private Timer _directionBufferTimer;

        [GameSerialize]
        private Vector2 _SavedGlobalPosition
        {
            get => GlobalPosition;
            set => GlobalPosition = value;
        }
        #endregion
    }
}