using Game.Plugins.CRR;
using Game.Utils;
using Godot;
using Godot.Collections;

namespace Game.Logic.Components
{
    [RegisterEditorResource("DamageEffectComponent", nameof(Node2D), nameof(Node2D), IconType.Editor)]
    public partial class DamageEffectComponent : DisposableNode2D
    {
        #region GAME METHODS
        [GameMethod]
        public override void _EnterTreeGame()
        {
            base._EnterTreeGame();

            Sprite.SetActive(false, false);

            _initialWaitTime = Timer.WaitTime;

            _subscriptions.Add(Timer, Timer.SignalName.Timeout, Callable.From(_OnDamageEffectTimerTimeout));
        }
        #endregion

        #region SIGNAL METHODS
        [SignalMethod]
        public void _OnDamageEffectTimerTimeout()
        {
            Sprite.SetActive(false, false);
        }

        [SignalMethod]
        public void _OnHealthComponentApplyDamage(Node from, float value, Dictionary parameters)
        {
            if (!Timer.IsStopped() || parameters != null && parameters.TryGetValue("NoDamageEffect", out var noDamageEffect)
                && (bool)noDamageEffect)
                return;

            Timer.WaitTime = _initialWaitTime;

            Sprite.SetActive(true, true);
            Timer.Start();
        }
        #endregion

        #region PUBLIC METHODS
        public void EmitDamageHit(float time = 0.0f)
        {
            Sprite.SetActive(true, true);
            Timer.WaitTime = time != 0.0f ? time : _initialWaitTime;
            Timer.Start();
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public Sprite2D Sprite;
        [Export]
        public Timer Timer;
        #endregion

        #region PRIVATE FIELDS
        private double _initialWaitTime;
        #endregion
    }
}