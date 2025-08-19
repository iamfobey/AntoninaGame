#if TOOLS

using Game.Utils;
using Godot;

namespace Game.Plugins.ExtendableInspector
{
    [Tool]
    public partial class InspectorPlugin : EditorInspectorPlugin
    {
        #region PUBLIC METHODS
        public override bool _CanHandle(GodotObject godotObject)
        {
            return true;
        }

        public override void _ParseBegin(GodotObject godotObject)
        {
            if (godotObject.HasMethod("InspectorBegin"))
            {
                godotObject.Call("InspectorBegin", this);
            }

            if (godotObject is Node node)
            {
                var btn = new CheckBox
                    { Text = "Enable/Disable Node", ButtonPressed = node.ProcessMode == Node.ProcessModeEnum.Inherit };
                btn.Pressed += () =>
                {
                    var value = node.ProcessMode;
                    node.ProcessMode =
                        value == Node.ProcessModeEnum.Inherit ? Node.ProcessModeEnum.Disabled : Node.ProcessModeEnum.Inherit;
                    if (node is CanvasItem canvasItem)
                        node.SetActive(node.ProcessMode == Node.ProcessModeEnum.Inherit, canvasItem.Visible);
                    else
                        node.SetActive(node.ProcessMode == Node.ProcessModeEnum.Inherit, true);
                };
                var color = Colors.DarkGray;
                btn.AddThemeStyleboxOverride("focus", new StyleBoxEmpty());
                btn.AddThemeColorOverride("font_focus_color", color);
                btn.AddThemeColorOverride("font_hover_color", color);
                btn.AddThemeColorOverride("font_hover_pressed_color", color);
                btn.AddThemeColorOverride("font_pressed_color", color);
                btn.AddThemeColorOverride("font_normal_color", color);
                btn.AddThemeColorOverride("font_color", color);
                AddCustomControl(btn);
            }

            if (godotObject is Node2D node2d)
            {
                var btn = new CheckBox { Text = "Show/Hide Node", ButtonPressed = node2d.Visible };
                btn.Pressed += () =>
                {
                    node2d.Visible = !node2d.Visible;
                };
                var color = Colors.DarkGray;
                btn.AddThemeStyleboxOverride("focus", new StyleBoxEmpty());
                btn.AddThemeColorOverride("font_focus_color", color);
                btn.AddThemeColorOverride("font_hover_color", color);
                btn.AddThemeColorOverride("font_hover_pressed_color", color);
                btn.AddThemeColorOverride("font_pressed_color", color);
                btn.AddThemeColorOverride("font_normal_color", color);
                btn.AddThemeColorOverride("font_color", color);
                AddCustomControl(btn);
            }
        }

        public override void _ParseEnd(GodotObject godotObject)
        {
            if (godotObject.HasMethod("InspectorEnd"))
            {
                godotObject.Call("InspectorEnd", this);
            }
        }

        public override void _ParseCategory(GodotObject godotObject, string category)
        {
            if (godotObject.HasMethod("InspectorCategory"))
            {
                godotObject.Call("ExtendInspectorCategory", this, category);
            }
        }

        public override bool _ParseProperty(
            GodotObject godotObject,
            Variant.Type type,
            string name,
            PropertyHint hintType,
            string hintString,
            PropertyUsageFlags usageFlags,
            bool wide
        )
        {
            if (godotObject.HasMethod("InspectorProperty"))
            {
                return godotObject.Call("InspectorProperty", this, (long)type, name, (long)hintType, hintString,
                    (long)usageFlags, wide).AsBool();
            }

            return false;
        }

        public override void _ParseGroup(GodotObject godotObject, string group)
        {
            if (godotObject.HasMethod("InspectorGroup"))
            {
                godotObject.Call("InspectorGroup", this, group);
            }
        }
        #endregion
    }
}
#endif