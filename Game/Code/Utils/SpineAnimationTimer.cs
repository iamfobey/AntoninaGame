using Game.Core.Logger;
using Godot;

namespace Game.Utils
{
    public class SpineAnimationTimer : DisposableNode
    {
        public SpineAnimationTimer(Callable callable)
        {
            Callable.From(() =>
            {
                Timer = this.CreateAndAddChild(new Timer());
                Timer.Autostart = false;
                Timer.OneShot = true;

                _subscriptions.Add(Timer, Timer.SignalName.Timeout, callable);
            }).CallDeferred();
        }

        #region PUBLIC METHODS
        public void Start(SpineAnimationManager spineAnimationManager, float speed = 1.0f)
        {
            float time = (spineAnimationManager.SpineAnimationState.GetCurrent(0).GetAnimationEnd() - 0.15f) / speed;
            Timer.Start(time);
        }

        public void Stop()
        {
            Timer.Stop();
        }
        #endregion

        #region PUBLIC FIELDS
        public Timer Timer;
        #endregion
    }
}