using Godot;

namespace Game.Utils
{
    public partial class DisposableCharacterBody2D : CharacterBody2D
    {
        #region GAME METHODS
        [GameMethod]
        public override void _ExitTreeGame()
        {
            base._ExitTreeGame();

            _subscriptions.Dispose();
        }
        #endregion

        protected readonly CompositeDisposable _subscriptions = new();
    }

    public partial class DisposableNode : Node
    {
        #region GAME METHODS
        [GameMethod]
        public override void _ExitTreeGame()
        {
            base._ExitTreeGame();

            _subscriptions.Dispose();
        }
        #endregion

        protected readonly CompositeDisposable _subscriptions = new();
    }

    public partial class DisposableNode2D : Node2D
    {
        #region GAME METHODS
        [GameMethod]
        public override void _ExitTreeGame()
        {
            base._ExitTreeGame();

            _subscriptions.Dispose();
        }
        #endregion

        protected readonly CompositeDisposable _subscriptions = new();
    }

    public partial class DisposableRigidBody2D : RigidBody2D
    {
        #region GAME METHODS
        [GameMethod]
        public override void _ExitTreeGame()
        {
            base._ExitTreeGame();

            _subscriptions.Dispose();
        }
        #endregion

        protected readonly CompositeDisposable _subscriptions = new();
    }

    public partial class DisposableSprite2D : Sprite2D
    {
        #region GAME METHODS
        [GameMethod]
        public override void _ExitTreeGame()
        {
            base._ExitTreeGame();

            _subscriptions.Dispose();
        }
        #endregion

        protected readonly CompositeDisposable _subscriptions = new();
    }
}