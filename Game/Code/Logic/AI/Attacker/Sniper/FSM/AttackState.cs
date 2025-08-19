using System;
using Game.Core.FSM;
using Game.Logic.Misc;
using Game.Utils;
using Godot;

namespace Game.Logic.AI.Attacker.Sniper.FSM
{
    [Tool]
    public partial class AttackState : State
    {
        #region ENUMS
        public enum EAttackWaitType
        {
            PreWait,
            AnimationWait,
            PostWait
        }
        #endregion

        #region FSM STATE METHODS
        [StateMethod]
        public override void _StateReady()
        {
            base._StateReady();

            _character = Root<Character>();

            _laserColor = ((ShaderMaterial)Laser.Material).GetShaderParameter("color").AsColor();
            _laserOutlineColor = ((ShaderMaterial)Laser.Material).GetShaderParameter("outline_color").AsColor();

            _globalSubscriptions.Add(AttackTimer, Timer.SignalName.Timeout, Callable.From(_OnAttackTimerTimeout));
        }

        [StateMethod]
        public override void _StateEnter()
        {
            base._StateEnter();

            AttackWaitType = EAttackWaitType.PreWait;
            AttackTimer.Start(_character.PreAttackWaitTime);

            if (_tween != null)
                _tween.Kill();

            UpdateProjectile();

            Laser.GlobalPosition = _character.LaserSocket.GlobalPosition;
            Laser.AddPoint(Vector2.Zero);
            Laser.AddPoint(_laserProjectileDirection * _laserProjectileLength);

            _tween = GetTree().CreateTween();
            _tween.SetLoops(1);
            _tween.TweenMethod(Callable.From((float value) =>
            {
                _laserColor.A = value;
                ((ShaderMaterial)Laser.Material).SetShaderParameter("color", _laserColor);
                _laserOutlineColor.A = value;
                ((ShaderMaterial)Laser.Material).SetShaderParameter("outline_color", _laserOutlineColor);
            }), 0.0f, 1.0f, 1.5f);

            _character.AnimationTree.RequestAnimation("Transition", "Idle")
                .UpdateBlendPosition("Idle", _character.MoveComponent.Direction);

            _character.MoveComponent.Speed = 0.0f;
        }

        [StateMethod]
        public override void _StateProcess(double delta)
        {
            base._StateProcess(delta);

            UpdateProjectile();

            if (Laser.GetPointCount() > 0)
                Laser.SetPointPosition(1, _laserProjectileDirection * _laserProjectileLength);
        }


        [StateMethod]
        public override void _StateExit()
        {
            base._StateExit();

            Laser.ClearPoints();

            AttackTimer.Stop();
        }
        #endregion

        #region SIGNAL METHODS
        [SignalMethod]
        public void _OnAttackTimerTimeout()
        {
            switch (AttackWaitType)
            {
                case EAttackWaitType.PreWait:
                    _character.AnimationTree.RequestAnimation("Transition", "Attack")
                        .UpdateBlendPosition("Attack", _character.MoveComponent.Direction);

                    AttackWaitType = EAttackWaitType.AnimationWait;
                    AttackTimer.Start(_character.AnimationAttackWaitTime);
                    break;
                case EAttackWaitType.AnimationWait:
                    AttackWaitType = EAttackWaitType.PostWait;
                    AttackTimer.Start(_character.PostAttackWaitTime);
                    break;
                case EAttackWaitType.PostWait:
                    Laser.ClearPoints();

                    UpdateProjectile();

                    var projectile = (LaserProjectile)Projectile.Instantiate();
                    projectile.GlobalPosition = _character.LaserSocket.GlobalPosition;
                    projectile.Direction = _laserProjectileDirection;
                    projectile.Length = _laserProjectileLength;
                    projectile.Initialize();
                    projectile.OnAreaBodyEntered += _OnProjectileAreaBodyEntered;
                    GetTree().Root.AddChild(projectile, true);

                    _character.CanChangeNavigationPosition = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [SignalMethod]
        public void _OnProjectileAreaBodyEntered(Projectile self, Node2D body)
        {
            switch (body)
            {
                case AttackBody { Root: Player.Character playerCharacter, Type: "Head" }:
                    playerCharacter.HealthComponent.ApplyDamage(_character, _character.AttackDamageValue);
                    break;
                case AttackBody { Root: Core.AI.Character aiCharacter, Type: "All" }:
                {
                    if (aiCharacter != _character && _character.ShouldProjectileDestroyOnAI)
                        self.QueueFree();
                    break;
                }
            }
        }
        #endregion

        #region PRIVATE METHODS
        private void UpdateProjectile(bool head = true)
        {
            if (head)
            {
                _laserProjectileDirection = (Player.Character.Instance.HeadSocket.GlobalPosition - _character.LaserSocket.GlobalPosition)
                    .Normalized();
                _laserProjectileLength =
                    _character.LaserSocket.GlobalPosition.DistanceTo(Player.Character.Instance.HeadSocket.GlobalPosition) + 15.0f;
            }
            else
            {
                _laserProjectileDirection = (Player.Character.Instance.GlobalPosition - _character.LaserSocket.GlobalPosition).Normalized();
                _laserProjectileLength = _character.LaserSocket.GlobalPosition.DistanceTo(Player.Character.Instance.GlobalPosition) + 45.0f;
            }
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public Timer AttackTimer;
        [Export]
        public PackedScene Projectile;
        [Export]
        public Line2D Laser;
        #endregion

        #region PUBLIC FIELDS
        public EAttackWaitType AttackWaitType = EAttackWaitType.PreWait;
        #endregion

        #region PRIVATE FIELDS
        private Character _character;
        private Tween _tween;
        private Vector2 _laserProjectileDirection;
        private float _laserProjectileLength;

        private Color _laserColor;
        private Color _laserOutlineColor;
        #endregion
    }
}