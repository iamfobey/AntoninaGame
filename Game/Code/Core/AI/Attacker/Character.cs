using Game.Core.Logger;
using Game.Core.Serialize;
using Game.Logic.Components;
using Game.Logic.Interactive;
using Game.Utils;
using Godot;
using Godot.Collections;

namespace Game.Core.AI.Attacker
{
    [GameSerializable]
    public partial class Character : AI.Character
    {
        #region GAME METHODS
        [GameMethod]
        public override void _ReadyGame()
        {
            base._ReadyGame();

            _subscriptions.Add(HealthComponent, HealthComponent.SignalName.OnApplyDamage,
                Callable.From<Node, float, Dictionary>(PoisonedComponent._OnHealthComponentApplyDamage));
            _subscriptions.Add(HealthComponent, HealthComponent.SignalName.OnApplyDamage,
                Callable.From<Node, float, Dictionary>(DamageEffectComponent._OnHealthComponentApplyDamage));

            _subscriptions.Add(HealthComponent, HealthComponent.SignalName.OnApplyDamage,
                Callable.From<Node, float, Dictionary>(_OnHealthComponentApplyDamage));
            _subscriptions.Add(HealthComponent, HealthComponent.SignalName.OnApplyDamage,
                Callable.From<Node, float, Dictionary>(_OnHealthComponentApplyHeal));

            HealthBar.Value = HealthComponent.CurrentHealth;
        }
        #endregion

        #region SIGNAL METHODS
        [SignalMethod]
        public virtual async void _OnHealthComponentApplyDamage(Node from, float value, Dictionary parameters)
        {
            HealthBar.Value = HealthComponent.CurrentHealth;

            if (from is Logic.Player.Character)
            {
                if (CanRemovePoisonOnHits && PoisonedComponent.IsPoisoned && PoisonedComponent.HitCount % HitCountToPoison == 0)
                {
                    PoisonedComponent.Undo(this);

                    _OnRemovePoisonBeforeTimer();
                    DamageEffectComponent.EmitDamageHit(WaitTimeWhenChangeToNormal);
                    await ToSignal(GetTree().CreateTimer(WaitTimeWhenChangeToNormal), SceneTreeTimer.SignalName.Timeout);
                    _OnRemovePoisonAfterTimer();
                }
            }
        }

        [SignalMethod]
        public virtual void _OnHealthComponentApplyHeal(Node from, float value, Dictionary parameters)
        {
            HealthBar.Value = HealthComponent.CurrentHealth;
        }

        [SignalMethod]
        public virtual async void _OnDirtEntered(Dirt dirt, bool inner)
        {
            if (CanChangeSelfToPoisoned && dirt.Type == Dirt.EType.Poison && inner && !PoisonedComponent.IsPoisoned)
            {
                PoisonedComponent.Apply(this);
                
                _OnChangeSelfToPoisonBeforeTimer();
                await ToSignal(GetTree().CreateTimer(WaitTimeWhenGotPoisoned), SceneTreeTimer.SignalName.Timeout);
                _OnChangeSelfToPoisonAfterTimer();
            }
        }
        #endregion

        #region PUBLIC METHODS
        public virtual void _OnRemovePoisonBeforeTimer()
        {

        }

        public virtual void _OnRemovePoisonAfterTimer()
        {

        }

        public virtual void _OnChangeSelfToPoisonBeforeTimer()
        {

        }

        public virtual void _OnChangeSelfToPoisonAfterTimer()
        {

        }
        #endregion

        #region EDITOR FIELDS
        [ExportGroup("Node Refs")]
        [Export]
        public ProgressBar HealthBar;
        [ExportGroup("Components")]
        [Export]
        public PoisonedComponent PoisonedComponent;

        [ExportCategory("Parameters")]
        [ExportGroup("Character Logic")]
        [Export]
        public float WaitTimeWhenGotStun = 2.0f;
        [Export]
        public float WaitTimeWhenChangeToNormal = 2.0f;
        [Export]
        public float WaitTimeWhenGotPoisoned = 2.0f;
        [Export]
        public int HitCountToPoison = 3;
        [Export]
        public bool CanRemovePoisonOnHits = true;
        [Export]
        public bool CanChangeSelfToPoisoned = true;

        [ExportGroup("Components")]
        [ExportSubgroup("PoisonedComponent")]
        [Export]
        [SyncVar(nameof(PoisonedComponent))]
        public bool IsPoisoned = false;
        #endregion
    }
}