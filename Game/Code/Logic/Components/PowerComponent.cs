using Game.Plugins.CRR;
using Game.Utils;
using Godot;
using Godot.Collections;

namespace Game.Logic.Components
{
    [RegisterEditorResource("PowerComponent", nameof(Node2D), nameof(Node2D), IconType.Editor)]
    public partial class PowerComponent : Node2D
    {
        #region DELEGATES
        [Signal]
        public delegate void OnAddPowerEventHandler(Node from, float value, Dictionary parameters);

        [Signal]
        public delegate void OnUsePowerEventHandler(Node from, float value, Dictionary parameters);
        #endregion

        #region GAME METHODS
        [GameMethod]
        public override void _EnterTreeGame()
        {
            base._EnterTreeGame();

            _currentPower = 0.0f;
        }
        #endregion

        #region PUBLIC METHODS
        public void AddPower(Node from, float value, Dictionary parameters = null)
        {
            CurrentPower += value;

            EmitSignal(SignalName.OnAddPower, from, value, parameters);
        }

        public void UsePower(Node from, float value, Dictionary parameters = null)
        {
            CurrentPower -= value;

            EmitSignal(SignalName.OnUsePower, from, value, parameters);
        }

        public bool HasEnoughStamina(float value)
        {
            return CurrentPower >= value;
        }
        #endregion

        #region PUBLIC FIELDS
        public float CurrentPower
        {
            get => _currentPower;
            private set => _currentPower = Mathf.Clamp(value, 0f, MaxPower);
        }

        public float MaxPower = 100.0f;
        #endregion

        #region PRIVATE FIELDS
        private float _currentPower;
        #endregion
    }
}