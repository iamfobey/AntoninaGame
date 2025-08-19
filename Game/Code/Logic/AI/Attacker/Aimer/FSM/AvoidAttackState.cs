using Game.Core.FSM;
using Game.Logic.Misc;
using Game.Utils;
using Godot;

namespace Game.Logic.AI.Attacker.Aimer.FSM
{
    [Tool]
    public partial class AvoidAttackState : State
    {
        #region FSM STATE METHODS
        [StateMethod]
        public override void _StateReady()
        {
            base._StateReady();

            _character = Root<Character>();
        }

        [StateMethod]
        public override void _StateEnter()
        {
            base._StateEnter();
            
            _character.MoveComponent.Speed = 0.0f;

            var projectile = (AimProjectile)Projectile.Instantiate();
            projectile.GlobalPosition = Player.Character.Instance.GlobalPosition;
            projectile.LifeTimer.WaitTime = 0.2f;
            projectile.OnLifeTimeout += self =>
            {
                var bodies = ((AimProjectile)self).Area.GetOverlappingBodies();

                foreach (var body in bodies)
                {
                    if (body is AttackBody { Root: Player.Character playerCharacter, Type: "All" })
                    {
                        playerCharacter.CanUpdateInput = false;
                        playerCharacter.MoveComponent.ApplyStun(this, 3.5f, () =>
                        {
                            playerCharacter.CanUpdateInput = true;
                        });
                        playerCharacter.HealthComponent.ApplyDamage(this, _character.AttackDamageValue);
                    }
                }
                self.QueueFree();
                
                _character.CanAttack = true;

                _character.NavigationAgent.TargetPosition =
                    _character.NavigationRegion.GetRandomPointRadius(GlobalPosition, _character.AttackRadius.X, _character.AttackRadius.Y);

                FSM.RequestState<RunState>();
            };
            projectile.Initialize();
            GetTree().Root.AddChild(projectile, true);
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public PackedScene Projectile;
        #endregion

        #region PRIVATE FIELDS
        private Character _character;
        #endregion
    }
}