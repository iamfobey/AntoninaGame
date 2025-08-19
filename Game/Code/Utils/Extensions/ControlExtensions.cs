using Godot;

namespace Game.Utils
{
    public static class ControlExtensions
    {
        #region STATIC METHODS
        // ReSharper disable once InconsistentNaming
        public static void InitControlForUI(this Control self)
        {
            self.SetDeferred(Control.PropertyName.Size, new Vector2(
                ProjectSettings.GetSetting("display/window/size/viewport_width").AsInt16(),
                ProjectSettings.GetSetting("display/window/size/viewport_height").AsInt16()));
            self.SetDeferred(Control.PropertyName.LayoutMode, 1);
            self.SetDeferred(Control.PropertyName.AnchorsPreset, (int)Control.LayoutPreset.FullRect);
            self.SetDeferred(Control.PropertyName.MouseFilter, (int)Control.MouseFilterEnum.Ignore);
        }
        #endregion
    }
}