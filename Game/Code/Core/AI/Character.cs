using Game.Core.Logger;
using Game.Core.Serialize;
using Game.Logic.Components;
using Game.Utils;
using Godot;

namespace Game.Core.AI
{
    [GameSerializable]
    public partial class Character : DisposableRigidBody2D
    {
        #region GAME METHODS
        [GameMethod]
        public override void _EnterTreeGame()
        {
            base._EnterTreeGame();

            Callable.From(() =>
            {
                Navigation.Server.Instance?.AIList.Add(this);
            }).CallDeferred();

            CustomIntegrator = false;
        }

        [GameMethod]
        public override void _ExitTreeGame()
        {
            base._ExitTreeGame();

            Callable.From(() =>
            {
                Navigation.Server.Instance?.AIList.Remove(this);
            }).CallDeferred();
        }

        [GameMethod]
        public override void _ReadyGame()
        {
            base._ReadyGame();

            if (AnimationPlayer != null)
                AnimationPlayer.Active = true;

            if (AnimationTree != null)
                AnimationTree.Active = true;

            if (ShouldInitNavAgentTargetPos)
                NavigationAgent.TargetPosition = GetNode<Node2D>("NavAgentInitTarget").GlobalPosition;

            NavigationAgent.AvoidanceEnabled = true;

            NavigationAgent.DebugEnabled = IsNavAgentDebugEnabled;

            NavigationAgent.TimeHorizonAgents = 3.0f;
            NavigationAgent.TimeHorizonObstacles = 1.0f;
            NavigationAgent.NeighborDistance = 1000.0f;
            NavigationAgent.Radius = 150.0f;

            NavigationAgent.PathDesiredDistance = 300.0f;
            NavigationAgent.TargetDesiredDistance = 200.0f;
            NavigationAgent.PathMaxDistance = 250.0f;

            NavigationAgent.SimplifyPath = true;
            NavigationAgent.SimplifyEpsilon = 4.0f;

            LinearDamp = 10.0f;
            AngularDamp = 5.0f;

            _subscriptions.Add(NavigationAgent, NavigationAgent2D.SignalName.VelocityComputed,
                Callable.From<Vector2>(_OnNavigationAgentVelocityComputed));

            HealthComponent.ApplyHeal(this, MaxHealth);
            StaminaComponent.RegenerateStamina(this, MaxStamina);
        }

        [GameMethod]
        public override void _PhysicsProcessGame(double delta)
        {
            base._PhysicsProcessGame(delta);

            if (IsUnderControlEffect)
            {
                ProcessControlEffects((float)delta);
            }
            else if (CanMoveAndSlide && IsNavAgentEnabled)
            {
                var currentAgentPosition = GlobalPosition;
                var nextPathPosition = NavigationAgent.GetNextPathPosition();

                if (currentAgentPosition.DistanceSquaredTo(nextPathPosition) >= 0.1f * 0.1f)
                {
                    var desiredVelocity = (nextPathPosition - currentAgentPosition).Normalized() * MoveComponent.Speed;
                    NavigationAgent.Velocity = desiredVelocity;

                    var directionForAnimation = Vector2.Zero;

                    if (SafeVelocity.LengthSquared() > 0.01f)
                        directionForAnimation = SafeVelocity.Normalized();
                    else if (desiredVelocity.LengthSquared() > 0.01f)
                        directionForAnimation = desiredVelocity.Normalized();

                    MoveComponent.Direction = !IsNavFinished()
                        ? MoveComponent.Direction.Lerp(directionForAnimation, (float)delta * 6.0f)
                        : Vector2.Zero;

                    MoveComponent.Velocity = desiredVelocity;
                    LinearVelocity = MoveComponent.Velocity;
                }
            }
        }
        #endregion

        #region SIGNAL METHODS
        [SignalMethod]
        public void _OnNavigationAgentVelocityComputed(Vector2 safeVelocity)
        {
            if (CanMoveAndSlide && IsNavAgentEnabled)
            {
                SafeVelocity = safeVelocity;
            }
        }
        #endregion

        #region PUBLIC METHODS
        public void ApplyHitReaction(Vector2 knockbackDirection, float knockbackForce, float launchForce, float knockbackTime = 0.45f)
        {
            if (IsUnderControlEffect) return;

            IsUnderControlEffect = true;
            CanMoveAndSlide = false;
            LinearVelocity = Vector2.Zero;

            if (launchForce > 0.0f)
            {
                _isLaunched = true;
                _zVelocity = launchForce;
            }

            if (knockbackForce > 0.0f)
            {
                _isKnockedBack = true;

                if (_knockbackTween != null && _knockbackTween.IsValid()) _knockbackTween.Kill();

                _knockbackTween = CreateTween();
                LinearVelocity = knockbackDirection * knockbackForce;

                _knockbackTween.TweenProperty(this, "linear_velocity", Vector2.Zero, knockbackTime)
                    .SetEase(Tween.EaseType.Out)
                    .SetTrans(Tween.TransitionType.Quad);

                _knockbackTween.Finished += () => { _isKnockedBack = false; };
            }
        }

        public bool IsNavFinished()
        {
            return NavigationAgent.IsNavigationFinished() && NavigationAgent.IsTargetReached() && IsNavAgentEnabled;
        }
        #endregion

        #region PRIVATE METHODS
        private void ProcessControlEffects(float delta)
        {
            if (_isLaunched)
            {
                float currentGravity = _zVelocity < 0 ? HANGTIME_GRAVITY : NORMAL_GRAVITY;
                _zVelocity -= currentGravity * delta;
        
                _zPos += _zVelocity * delta;
        
                AnimatedSprite.Position = new Vector2(AnimatedSprite.Position.X, -_zPos);

                if (_zPos <= 0.0f)
                {
                    _zPos = 0.0f;
                    _zVelocity = 0.0f;
                    _isLaunched = false;
                    AnimatedSprite.Position = Vector2.Zero;
                }
            }

            if (!_isLaunched && !_isKnockedBack)
            {
                IsUnderControlEffect = false;
                CanMoveAndSlide = true;
            }
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public SpineAnimationManager SpineAnimationManager;
        [Export]
        public AnimatedSprite2D AnimatedSprite;
        [Export]
        public AnimationPlayer AnimationPlayer;
        [Export]
        public AnimationTree AnimationTree;
        [Export]
        public CollisionShape2D CollisionShape;
        [Export]
        public NavigationAgent2D NavigationAgent;
        [Export]
        public NavigationRegion2D NavigationRegion;
        [Export]
        public FSM.FSM FSM;

        [ExportGroup("Components")]
        [Export]
        public HealthComponent HealthComponent;
        [Export]
        public StaminaComponent StaminaComponent;
        [Export]
        public MoveComponent MoveComponent;
        [Export]
        public DamageEffectComponent DamageEffectComponent;

        [ExportCategory("Parameters")]
        [ExportGroup("Logic")]
        [Export]
        public bool ShouldInitNavAgentTargetPos = true;
        [Export]
        public bool IsNavAgentDebugEnabled = true;
        [Export]
        public bool CanMoveAndSlide = true;
        [Export]
        public bool IsNavAgentEnabled = true;

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
        public Vector2 SafeVelocity;
        public bool IsUnderControlEffect = false;
        #endregion

        #region PRIVATE FIELDS
        private float _zPos = 0.0f;
        private float _zVelocity = 0.0f;
        private bool _isLaunched = false;
        private bool _isKnockedBack = false;
        private Tween _knockbackTween; // Поле для хранения твина
        private float _knockbackTimer = 0f;
        private Vector2 _knockbackVelocity = Vector2.Zero;
        [GameSerialize]
        private Vector2 _SavedGlobalPosition
        {
            get => GlobalPosition;
            set => GlobalPosition = value;
        }

        [GameSerialize]
        private Vector2 _SavedAgentTargetPosition
        {
            get => NavigationAgent.TargetPosition;
            set => NavigationAgent.TargetPosition = value;
        }
        #endregion

        private const float NORMAL_GRAVITY = 5000.0f; // Увеличим базовую гравитацию для резкого взлета
        private const float HANGTIME_GRAVITY = 2000.0f; // Уменьшенная гравитация для "зависания"
    }
}