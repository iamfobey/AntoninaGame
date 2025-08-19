using Game.Core.Logger;
using Game.Utils;
using Godot;
using Godot.Collections;

namespace Game.Logic.Interactive
{
    [Tool]
    public partial class Dirt : DisposableSprite2D
    {
        #region ENUMS
        public enum EType
        {
            Default = 0,
            Poison,
            Heal,
            Chemical
        }
        #endregion

        #region GAME METHODS
        [GameMethod]
        public override void _ReadyGame()
        {
            base._ReadyGame();

            _subscriptions.Add(OuterArea, Area2D.SignalName.BodyEntered, Callable.From((Node2D body) =>
            {
                Log.Debug("Dirt outer area entered");
                
                _OnDirtEntered(body, false);
            }));
            _subscriptions.Add(OuterArea, Area2D.SignalName.BodyExited, Callable.From((Node2D body) =>
            {
                Log.Debug("Dirt outer area entered");
                
                _OnDirtExited(body, false);
            }));

            _subscriptions.Add(InnerArea, Area2D.SignalName.BodyEntered, Callable.From((Node2D body) =>
            {
                Log.Debug("Dirt inner area entered");
                
                _OnDirtEntered(body, true);
            }));
            _subscriptions.Add(InnerArea, Area2D.SignalName.BodyExited, Callable.From((Node2D body) =>
            {
                Log.Debug("Dirt inner area exited");
                
                _OnDirtExited(body, true);
            }));
        }
        #endregion

        #region SIGNAL METHODS
        [SignalMethod]
        public void _OnDirtEntered(Node2D body, bool inner)
        {
            body.CallFunction("_OnDirtEntered", [this, inner]);
        }

        [SignalMethod]
        public void _OnDirtExited(Node2D body, bool inner)
        {
            body.CallFunction("_OnDirtExited", [this, inner]);
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        public Dictionary<EType, Texture2D> TypeTexture = new();
        [Export]
        public Area2D OuterArea;
        [Export]
        public Area2D InnerArea;

        [ExportCategory("Parameters")]
        [ExportGroup("Logic")]
        [Export]
        public EType Type
        {
            get => _type;
            set
            {
                _type = value;
                if (TypeTexture.TryGetValue(value, out var texture))
                    SetTexture(texture);
            }
        }
        #endregion

        #region PRIVATE FIELDS
        private EType _type = EType.Default;
        #endregion
    }
}