using Game.Utils;
using Godot;
using Godot.Collections;

namespace Game.Core.UI
{
    [Tool]
    public partial class TrTextureRect : Control
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

                var privateNode = this.CreateAndAddChild(new Control(), "Translation");

                string[] locales = TranslationServer.GetLoadedLocales();
                foreach (string locale in locales)
                {
                    var inst = privateNode.CreateAndAddChild(new TextureRect(), locale);
                    _cache.Add(inst);
                }

                SetMeta("tr_is_initialized", true);
            }
        }
        #endif
        #endregion
        #endregion

        #region PRIVATE METHODS
        private void UpdateState()
        {
            string currentLocale = TranslationServer.GetLocale();

            foreach (var rect in _cache)
            {
                rect.Hide();

                if (rect.Name == currentLocale)
                {
                    _currentRect?.Hide();
                    _currentRect = rect;
                    _currentRect.Show();
                }
            }
        }
        #endregion

        #region EDITOR FIELDS
        [ExportCategory("Node Refs")]
        [ExportGroup("Base")]
        [Export]
        private Array<TextureRect> _cache = [];
        #endregion

        #region PRIVATE FIELDS
        private TextureRect _currentRect = null;
        #endregion
    }
}