using Game.Utils;
using Godot;

namespace Game.Logic.Interactive 
{
    [Tool]
    public partial class Barrel : DisposableRigidBody2D
    {
        public enum EBarrelType
        {
            Explosive,
            Toxic,
            Water
        }

        [Export]
        public EBarrelType Type = EBarrelType.Explosive;
        [Export]
        public float ExplosionKnockbackForce = 8000.0f;
        [Export]
        public float PlayerDamageOnHit = 10.0f;

        [Export]
        public Area2D ExplosionArea;
        [Export]
        public CollisionShape2D ExplosionShape;
        public bool IsMoving = false;
        [Export]
        public float Friction = 2000.0f;

        public override void _PhysicsProcessGame(double delta)
        {
            if (IsMoving)
            {
                LinearVelocity = LinearVelocity.MoveToward(Vector2.Zero, Friction * (float)delta);
                MoveAndCollide(LinearVelocity * (float)delta);

                if (LinearVelocity == Vector2.Zero)
                {
                    IsMoving = false;
                    Callable.From(() =>
                    {
                        if (!IsMoving)
                            Explode();
                    }).CallDeferred();
                }
            }
        }

        public void HandleKick(Vector2 direction, float force)
        {
            LinearVelocity = direction * force;
            IsMoving = true;
        }

        public void Explode()
        {
            var bodies = ExplosionArea.GetOverlappingBodies();
            
            foreach (var body in bodies)
            {
                if (body is AttackBody { Root: Core.AI.Character aiCharacter, Type: "All" })
                {
                    var direction = (aiCharacter.GlobalPosition - GlobalPosition).Normalized();
                    aiCharacter.ApplyHitReaction(direction, ExplosionKnockbackForce, 0.0f, 0.5f);
                    aiCharacter.HealthComponent.ApplyDamage(this, 5.0f);
                    continue;
                }
                
                if (body is AttackBody { Root: Player.Character playerCharacter, Type: "All" })
                {
                    playerCharacter.HealthComponent.ApplyDamage(this, 5.0f);
                    continue;
                }
                
                if (body is AttackBody { Root: Barrel otherBarrel, Type: "All" } && otherBarrel != this)
                {
                    var direction = (otherBarrel.GlobalPosition - GlobalPosition).Normalized();
                    otherBarrel.HandleKick(direction, ExplosionKnockbackForce / 2);
                }
            }

            QueueFree();
        }
    }
}