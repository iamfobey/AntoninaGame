using Game.Logic.Player;
using Game.Utils;
using Godot;

namespace Game.Logic.UI
{
    public partial class HUD : Control
    {
        #region GAME METHODS
        [GameMethod]
        public override void _ProcessGame(double delta)
        {
            base._ProcessGame(delta);

            HealthBar.Value = Character.Instance.HealthComponent.CurrentHealth;
            StaminaBar.Value = Character.Instance.StaminaComponent.CurrentStamina;
            PowerBar.Value = Character.Instance.PowerComponent.CurrentPower;
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public ProgressBar HealthBar;
        [Export]
        public ProgressBar StaminaBar;
        [Export]
        public ProgressBar PowerBar;
        #endregion
    }
}