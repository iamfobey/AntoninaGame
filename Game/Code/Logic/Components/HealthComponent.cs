using Game.Plugins.CRR;
using Game.Utils;
using Godot;
using Godot.Collections;

namespace Game.Logic.Components
{
    [RegisterEditorResource("HealthComponent", nameof(Node2D), nameof(Node2D), IconType.Editor)]
    public partial class HealthComponent : Node2D
    {
        #region DELEGATES
        [Signal]
        public delegate void OnApplyDamageEventHandler(Node from, float value, Dictionary parameters);

        [Signal]
        public delegate void OnApplyHealEventHandler(Node from, float value, Dictionary parameters);
        #endregion

        #region GAME METHODS
        [GameMethod]
        public override void _EnterTreeGame()
        {
            base._ReadyGame();

            _currentHealth = MaxHealth;
        }

        [GameMethod]
        public override void _ProcessGame(double delta)
        {
            base._ProcessGame(delta);

            if (CanRegenerateHealth && CurrentHealth < MaxHealth)
            {
                ApplyHeal(this, HealthRegenerationRate * (float)delta);
            }
        }
        #endregion

        #region PUBLIC METHODS
        public void ApplyDamage(Node from, float value, Dictionary parameters = null)
        {
            if (!CanReceiveDamage)
                return;

            CurrentHealth -= value;
            DamageHitCount++;

            EmitSignal(SignalName.OnApplyDamage, from, value, parameters);
        }

        public void ApplyHeal(Node from, float value, Dictionary parameters = null)
        {
            if (!CanReceiveHeal)
                return;

            CurrentHealth += value;
            HealHitCount++;

            EmitSignal(SignalName.OnApplyHeal, from, value, parameters);
        }
        #endregion

        #region PUBLIC FIELDS
        public float CurrentHealth
        {
            get => _currentHealth;
            private set => _currentHealth = Mathf.Clamp(value, 0f, MaxHealth);
        }

        public int DamageHitCount = 0;
        public int HealHitCount = 0;
        public float MaxHealth = 100.0f;
        public float HealthRegenerationRate = 5.0f;
        public bool CanRegenerateHealth = false;
        public bool CanReceiveDamage = true;
        public bool CanReceiveHeal = true;
        #endregion

        #region PRIVATE FIELDS
        private float _currentHealth;
        #endregion
    }
}