using Game.Core.FSM;
using Game.Core.Input;
using Game.Logic.Interactive;
using Game.Logic.Misc;
using Game.Utils;
using Godot;

namespace Game.Logic.Player.FSM
{
    [Tool]
    public partial class RagAttackState : State
    {
        #region FSM STATE METHODS
        [StateMethod]
        public override void _StateReady()
        {
            base._StateReady();
            _character = Root<Character>();
            _camera = _character.Camera; 
        }

        [StateMethod]
        public override void _StateEnter()
        {
            base._StateEnter();
            
            _character.CanUpdateInput = false;
            _character.MoveComponent.Speed = 0.0f;
            
            Line.ClearPoints();
            Line.AddPoint(Vector2.Zero);
            Line.AddPoint(Vector2.Zero);
        }

        [StateMethod]
        public override void _StateProcess(double delta)
        {
            base._StateProcess(delta);

            Vector2 mousePosition = _character.GetGlobalMousePosition();
            Vector2 characterCenter = _character.MiddleSocket.GlobalPosition;

            Line.GlobalPosition = characterCenter;
            Line.SetPointPosition(0, Vector2.Zero);
            Line.SetPointPosition(1, (mousePosition - characterCenter));
            
            Vector2 aimDirection = (mousePosition - characterCenter).Normalized();
            
            _character.SpineAnimationManager.SetAnimationByDirection("Attack", aimDirection);

            Vector2 cameraOffsetVector = mousePosition - characterCenter;
                
            if (cameraOffsetVector.Length() > MaxCameraOffset)
            {
                cameraOffsetVector = cameraOffsetVector.Normalized() * MaxCameraOffset;
            }
                
            _camera.Offset = _camera.Offset.Lerp(cameraOffsetVector, (float)delta * CameraSmoothing);
        }
        
        [StateMethod]
        public override void _StateExit()
        {
            base._StateExit();

            Line.ClearPoints();
            
            _character.CanUpdateInput = true;
        }

        [StateMethod]
        public override void _StateProcessPhysics(double delta)
        {
            base._StateProcessPhysics(delta);
            
            if (Server.IsActionJustReleased(Map.PlayerRagAttack, true))
            {
                ExecuteAttack();
                
                if (_character.MoveComponent.Direction == Vector2.Zero)
                    FSM.RequestState<IdleState>();
                else
                    FSM.RequestState<WalkState>();                    
                return;
            }
        }
        #endregion

        private void ExecuteAttack()
        {
            Vector2 mousePosition = _character.GetGlobalMousePosition();
            Vector2 characterCenter = _character.MiddleSocket.GlobalPosition;
            Vector2 fireDirection = (mousePosition - characterCenter).Normalized();

            var projectile = (StraightProjectile)Projectile.Instantiate();
            projectile.GlobalPosition = characterCenter;
            projectile.Direction = fireDirection;
            projectile.Rotation = projectile.Direction.Angle();
            projectile.Speed = 1000.0f;
            projectile.Initialize();
            projectile.OnAreaBodyEntered += (self, body) =>
            {
                switch (body)
                {
                    case AttackBody { Root: Barrel barrel, Type: "All" }:
                        barrel.Explode();
                        self.QueueFree();
                        break;
                    case AttackBody { Root: Core.AI.Character aiCharacter, Type: "All" }:
                        aiCharacter.MoveComponent.ApplyStun(_character, 3.5f);
                        self.QueueFree();
                        break;
                }
            };
            GetTree().Root.AddChild(projectile, true);
        }

        #region EXPORTED NODE REFS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public Line2D Line;
        [Export]
        public PackedScene Projectile;
        
        [ExportCategory("Parameters")]
        [ExportGroup("Logic")]
        [Export]
        public float MaxCameraOffset { get; set; } = 300.0f; 
        [Export]
        public float CameraSmoothing { get; set; } = 5.0f;
        #endregion
        
        #region PRIVATE FIELDS
        private Character _character;
        private Camera2D _camera;
        #endregion
    }
}