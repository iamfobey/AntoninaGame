using System.Collections.Generic;
using Game.Core.AI;
using Godot;

namespace Game.Core.Navigation
{
    public partial class Server : Node
    {
        #region PUBLIC METHODS
        public override void _EnterTreeGame()
        {
            base._EnterTreeGame();

            Instance = this;
        }
        #endregion

        #region STATIC FIELDS
        public static Server Instance { get; private set; }
        #endregion

        #region PUBLIC FIELDS
        public List<Character> AIList = [];
        #endregion
    }
}