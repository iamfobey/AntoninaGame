using Game.Utils;
using Godot;
using Godot.Collections;

namespace Game.Core.UI
{
    [Tool]
    public partial class TrTextureButton : Control
    {
        #region GAME METHODS
        [GameMethod]
        public override void _ReadyGame()
        {
            base._ReadyGame();

            UpdateState();
        }

        [GameMethod]
        public override void _Notification(int what)
        {
            base._Notification(what);

            if (what == NotificationTranslationChanged)
            {
                UpdateState();
            }

            #if TOOLS
            if (what == NotificationEditorPostSave)
            {
                UpdateState();
            }
            #endif
        }
        #endregion

        #region PUBLIC METHODS
        #region EDITOR METHODS
        #if TOOLS
        [EditorMethod]
        public override void _ReadyEditor()
        {
            base._ReadyEditor();

            if (Engine.IsEditorHint() && (!HasMeta("tr_is_initialized") || !GetMeta("tr_is_initialized").AsBool()))
            {
                this.InitControlForUI();

                var privateControl = this.CreateAndAddChild(new Control(), "Translation");
                privateControl.InitControlForUI();

                string[] locales = TranslationServer.GetLoadedLocales();
                foreach (string locale in locales)
                {
                    var inst = privateControl.CreateAndAddChild(new TextureButton(), locale);
                    _cache.Add(inst);
                }

                SetMeta("tr_is_initialized", true);
            }

            UpdateState();
        }
        #endif
        #endregion
        #endregion

        #region PRIVATE METHODS
        private void UpdateState()
        {
            string currentLocale = TranslationServer.GetLocale();

            foreach (var button in _cache)
            {
                if (button.Name == currentLocale)
                {
                    _currentButton?.Hide();
                    _currentButton = button;
                    _currentButton.Show();
                }
            }
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        private Array<TextureButton> _cache = [];
        #endregion

        #region PRIVATE FIELDS
        private TextureButton _currentButton = null;
        #endregion
    }
}