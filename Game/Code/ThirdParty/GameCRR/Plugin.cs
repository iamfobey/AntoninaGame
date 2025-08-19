#if TOOLS

using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using System.Linq;
using System.Text.RegularExpressions;
using Game.Utils;

namespace Game.Plugins.CRR
{
    [Tool]
    public partial class Plugin : EditorPlugin
    {
        #region STATIC METHODS
        private static List<string> FindAllClassPaths(Type type, string directory)
        {
            var paths = new List<string>();

            using var dir = DirAccess.Open(directory);
            
            if (dir == null)
            {
                GD.PrintErr($"[CRR Plugin] FindAllClassPaths: Could not open directory: {directory}. Godot Error: {DirAccess.GetOpenError()}");
                return paths;
            }

            dir.ListDirBegin();

            while (true)
            {
                string fileOrDirName = dir.GetNext();

                if (string.IsNullOrEmpty(fileOrDirName))
                    break;

                if (fileOrDirName is "." or "..")
                    continue;
                    
                string currentItemPath;
                string currentDir = dir.GetCurrentDir();

                if (currentDir.EndsWith("/"))
                {
                    currentItemPath = currentDir + fileOrDirName;
                }
                else
                {
                    currentItemPath = currentDir + "/" + fileOrDirName;
                }

                if (dir.CurrentIsDir())
                {
                    paths.AddRange(FindAllClassPaths(type, currentItemPath));
                }
                    
                else if (fileOrDirName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    if (VerifyTypeMatchesScript(type, currentItemPath))
                    {
                        paths.Add(currentItemPath);
                    }
                }
            }
            return paths;
        }

        private static bool VerifyTypeMatchesScript(Type type, string scriptPath)
        {
            try
            {
                if (!FileAccess.FileExists(scriptPath))
                {
                    GD.PrintErr($"[CRR Plugin] VerifyTypeMatchesScript: File does not exist at path: {scriptPath}");
                    return false;
                }

                using var file = FileAccess.Open(scriptPath, FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    GD.PrintErr($"[CRR Plugin] VerifyTypeMatchesScript: Failed to open script at {scriptPath}. Godot Error: {FileAccess.GetOpenError()}");
                    return false;
                }

                string scriptSource = file.GetAsText();
                if (string.IsNullOrWhiteSpace(scriptSource))
                {
                    GD.PushWarning($"[CRR Plugin] VerifyTypeMatchesScript: Script at {scriptPath} is empty or whitespace.");
                    return false;
                }

                string pattern = $@"(^|\s)(public\s+|internal\s+|protected\s+|private\s+)?(static\s+)?(abstract\s+|sealed\s+)?(partial\s+)?class\s+{Regex.Escape(type.Name)}(\s*[:<{{])";
                
                bool found = Regex.IsMatch(scriptSource, pattern);

                return found;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[CRR Plugin] VerifyTypeMatchesScript: Error reading script at {scriptPath}: {ex.Message}");
                return false;
            }
        }

        private static IEnumerable<Type> GetCustomRegisteredTypes()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t is { IsAbstract: false, IsClass: true }
                    && Attribute.IsDefined(t, typeof(RegisterEditorResourceAttribute))
                    && (t.IsSubclassOf(typeof(Node)) || t.IsSubclassOf(typeof(Resource)))
                );
        }
        #endregion

        #region PUBLIC METHODS
        [EditorMethod]
        public override void _EnterTreeEditor()
        {
            base._EnterTreeEditor();
            
            RefreshCustomClasses();
        }

        [EditorMethod]
        public override void _ApplyChanges()
        {
            base._ApplyChanges();
            
            RefreshCustomClasses();
        }

        [EditorMethod]
        public override void _ExitTreeEditor()
        {
            base._ExitTreeEditor();
            
            UnregisterCustomClasses();
        }
        #endregion

        #region PRIVATE METHODS
        private void RefreshCustomClasses()
        {
            UnregisterCustomClasses();
            RegisterCustomClasses();
        }

        private void RegisterCustomClasses()
        {
            var registeredTypes = GetCustomRegisteredTypes();

            foreach (var type in registeredTypes)
            {
                var paths = FindAllClassPaths(type, "res://Game/Code/");

                foreach (string path in paths)
                {
                    var attribute =
                        Attribute.GetCustomAttribute(type, typeof(RegisterEditorResourceAttribute)) as RegisterEditorResourceAttribute;
                    
                    var script = GD.Load<Script>(path);
                    if (script == null)
                    {
                        GD.PrintErr($"[CRR Plugin] RegisterCustomClasses: Failed to load script for type '{type.FullName}' at path '{path}'. Skipping this script.");
                        continue;
                    }
                    
                    ImageTexture icon = null;
                    if (attribute != null)
                    {
                        string baseType;

                        if (!string.IsNullOrEmpty(attribute.BaseType))
                        {
                            baseType = attribute.BaseType;
                        }
                        else
                        {
                            baseType = type.IsSubclassOf(typeof(Resource)) ? nameof(Resource) : nameof(Node);
                        }

                        if (!string.IsNullOrEmpty(attribute.IconPath))
                        {
                            switch (attribute.IconType)
                            {
                                case IconType.Custom:
                                    if (FileAccess.FileExists(attribute.IconPath))
                                    {
                                        var rawIcon = ResourceLoader.Load<Texture2D>(attribute.IconPath);
                                        if (rawIcon != null)
                                        {
                                            var image = rawIcon.GetImage();
                                            
                                            if (EditorInterface.Singleton != null)
                                            {
                                                int length = (int)Mathf.Round(16 * EditorInterface.Singleton.GetEditorScale());
                                                image.Resize(length, length);
                                                icon = ImageTexture.CreateFromImage(image);
                                            }
                                            else
                                            {
                                                GD.PushWarning($"[CRR Plugin] EditorInterface.Singleton is null. Cannot resize icon for {type.FullName}. Using original size.");
                                                icon = ImageTexture.CreateFromImage(image); // Или пропустить установку иконки
                                            }
                                        }
                                        else
                                        {
                                            GD.PrintErr(
                                                $"[CRR Plugin] Could not load the icon for the registered type \"{type.FullName}\" at path \"{attribute.IconPath}\".");
                                        }
                                    }
                                    else
                                    {
                                        GD.PrintErr(
                                            $"[CRR Plugin] The icon path \"{attribute.IconPath}\" for the registered type \"{type.FullName}\" does not exist.");
                                    }
                                    break;
                                case IconType.Editor:
                                    if (EditorInterface.Singleton != null && EditorInterface.Singleton.GetBaseControl() != null)
                                    {
                                        var editorIcon = EditorInterface.Singleton.GetBaseControl().GetThemeIcon(attribute.IconPath, "EditorIcons");
                                        if (editorIcon != null)
                                        {
                                            icon = ImageTexture.CreateFromImage(editorIcon.GetImage());
                                        }
                                        else
                                        {
                                            GD.PrintErr($"[CRR Plugin] Could not get editor icon '{attribute.IconPath}' for type '{type.FullName}'.");
                                        }
                                    }
                                    else
                                    {
                                         GD.PushWarning($"[CRR Plugin] EditorInterface or BaseControl is null. Cannot get editor icon for {type.FullName}.");
                                    }
                                    break;
                                default:
                                    GD.PrintErr($"[CRR Plugin] Unknown IconType: {attribute.IconType} for type '{type.FullName}'.");
                                    break;
                            }
                        }
                        
                        AddCustomType(attribute.Name, baseType, script, icon);
                        _customTypes.Add(attribute.Name);
                    }
                    else
                    {
                        GD.PushWarning($"[CRR Plugin] RegisterEditorResourceAttribute not found for type '{type.FullName}' (script: '{path}'), though it passed GetCustomRegisteredTypes. This shouldn't happen.");
                    }
                }
            }
        }

        private void UnregisterCustomClasses()
        {
            foreach (string typeName in _customTypes)
            {
                RemoveCustomType(typeName);
            }
            _customTypes.Clear();
        }
        #endregion

        #region PRIVATE FIELDS
        private readonly List<string> _customTypes = [];
        #endregion
    }
}

#endif