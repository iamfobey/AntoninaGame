using Game.Utils;
using Godot;

namespace Game.Core.Debug
{
    public partial class Server : Node
    {
        #region GAME METHODS
        [GameMethod]
        public override void _EnterTreeGame()
        {
            base._EnterTreeGame();

            Instance = this;

            _menu = this.CreateAndAddChild(new Menu(), "Menu");
        }

        [GameMethod]
        public override void _InputGame(InputEvent @event)
        {
            base._InputGame(@event);

            if (Input.Server.IsActionJustPressed("Debug_Menu"))
            {
                _menu.Enabled = !_menu.Enabled;
            }
        }
        #endregion

        #region STATIC FIELDS
        public static Server Instance { get; private set; }
        #endregion

        #region PRIVATE FIELDS
        private Menu _menu;
        #endregion
    }
}