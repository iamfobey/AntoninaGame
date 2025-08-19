#if TOOLS
using Game.Utils;
using Godot;

namespace Game.Plugins.ExtendableInspector
{
    [Tool]
    public partial class Plugin : EditorPlugin
    {
        #region PUBLIC METHODS
        [EditorMethod]
        public override void _EnterTreeEditor()
        {
            _instance = new InspectorPlugin();
            AddInspectorPlugin(_instance);
        }

        [EditorMethod]
        public override void _ExitTreeEditor()
        {
            RemoveInspectorPlugin(_instance);
        }
        #endregion

        #region PRIVATE FIELDS
        private InspectorPlugin _instance;
        #endregion
    }
}
#endif