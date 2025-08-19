using Game.Utils;
using Godot;
using Godot.Collections;

namespace Game.Core
{
    [Tool]
    public partial class Server : Node
    {
        #region GAME METHODS
        [GameMethod]
        public override void _EnterTreeGame()
        {
            base._EnterTreeGame();

            Instance = this;

            foreach (string arg in OS.GetCmdlineArgs())
            {
                if (arg.Contains('='))
                {
                    string[] keyValue = arg.Split("=");
                    CmdlineArgs[keyValue[0].TrimPrefix("--")] = keyValue[1];
                }
                else
                {
                    CmdlineArgs[arg.TrimPrefix("--")] = "";
                }
            }

            bool sceneChanged = false;
            foreach (var arg in CmdlineArgs)
            {
                if (arg.Key == "scene")
                {
                    Callable.From(() =>
                    {
                        GetTree().ChangeSceneToFile("res://Game/Content/Scenes/" + arg.Value + ".tscn");
                    }).CallDeferred();
                    sceneChanged = true;
                }
            }

            if (!sceneChanged)
                Callable.From(() => { GetTree().ChangeSceneToFile(ProjectSettings.GetSetting("application/run/scene").AsString()); })
                    .CallDeferred();
        }

        [GameMethod]
        public override void _InputGame(InputEvent @event)
        {
            base._InputGame(@event);

            // if (Input.Server.IsActionJustPressed(Map.GameSave))
            // {
            // SaveLoad.SaveGame(SavePath);
            // }

            // if (Input.Server.IsActionJustPressed(Map.GameLoad))
            // {
            // SaveLoad.LoadGame(SavePath);
            // }
        }
        #endregion

        #region PUBLIC METHODS
        #if TOOLS
        [EditorMethod]
        public override void _EnterTreeEditor()
        {
            base._EnterTreeEditor();

            if (!ProjectSettings.HasSetting("application/run/scene"))
            {
                ProjectSettings.SetSetting("application/run/scene", "res://Game/Content/Scenes/");
                var propertyInfo = new Dictionary
                {
                    { "name", "application/run/scene" },
                    { "type", (int)Variant.Type.String },
                    { "hint", (int)PropertyHint.File },
                    { "hint_string", "*.tscn" }
                };
                ProjectSettings.AddPropertyInfo(propertyInfo);
            }
        }
        #endif
        #endregion

        #region STATIC FIELDS
        public static Server Instance { get; private set; }
        #endregion

        #region PUBLIC FIELDS
        public System.Collections.Generic.Dictionary<string, string> CmdlineArgs = [];
        #endregion

        #region PRIVATE FIELDS
        private string SavePath => "user://Saved/Saves/save.json";
        #endregion
    }
}