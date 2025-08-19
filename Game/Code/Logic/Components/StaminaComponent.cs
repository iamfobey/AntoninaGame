using Game.Plugins.CRR;
using Game.Utils;
using Godot;
using Godot.Collections;

namespace Game.Logic.Components
{
    [RegisterEditorResource("StaminaComponent", nameof(Node2D), nameof(Node2D), IconType.Editor)]
    public partial class StaminaComponent : Node2D
    {
        #region DELEGATES
        [Signal]
        public delegate void OnRegenerateStaminaEventHandler(Node from, float value, Dictionary parameters);

        [Signal]
        public delegate void OnUseStaminaEventHandler(Node from, float value, Dictionary parameters);
        #endregion

        #region GAME METHODS
        [GameMethod]
        public override void _EnterTreeGame()
        {
            base._EnterTreeGame();

            _currentStamina = MaxStamina;
        }

        [GameMethod]
        public override void _ProcessGame(double delta)
        {
            base._ProcessGame(delta);

            if (CanRegenerateStamina)
            {
                if (CurrentStamina < MaxStamina)
                {
                    _regenerationRateMultiplayer *= StaminaRegenerationRateMultiplayer;
                    RegenerateStamina(this, StaminaRegenerationRate * _regenerationRateMultiplayer * (float)delta);
                }
                else
                {
                    _regenerationRateMultiplayer = 1.0f;
                }
            }
        }
        #endregion

        #region PUBLIC METHODS
        public void UseStamina(Node from, float value, Dictionary parameters = null)
        {
            CurrentStamina -= value;

            _regenerationRateMultiplayer = 1.0f;

            EmitSignal(SignalName.OnUseStamina, from, value, parameters);
        }

        public void RegenerateStamina(Node from, float value, Dictionary parameters = null)
        {
            CurrentStamina += value;

            EmitSignal(SignalName.OnRegenerateStamina, from, value, parameters);
        }

        public bool HasEnoughStamina(float value)
        {
            return CurrentStamina >= value;
        }
        #endregion

        #region PUBLIC FIELDS
        public float CurrentStamina
        {
            get => _currentStamina;
            private set => _currentStamina = Mathf.Clamp(value, 0f, MaxStamina);
        }

        public float MaxStamina = 100.0f;
        public float StaminaRegenerationRate = 10.0f;
        public float StaminaRegenerationRateMultiplayer = 1.005f;
        public bool CanRegenerateStamina = false;
        #endregion

        #region PRIVATE FIELDS
        private float _currentStamina;
        private float _regenerationRateMultiplayer = 1.0f;
        #endregion
    }
}