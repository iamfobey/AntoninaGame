using System;
using Game.Core.Logger;
using Godot;
using FileAccess = Godot.FileAccess;

namespace Game.Core
{
    public static class SaveLoad
    {
        #region STATIC METHODS
        public static void SaveGame(string path)
        {
            if (Serialize.Server.Instance == null)
            {
                Log.Error("Cannot save game: Serialize.Server.Instance is null.", ELogCategory.SaveLoad);
                return;
            }

            Log.Info($"Attempting to save game to: {path}", ELogCategory.SaveLoad);
            try
            {
                string jsonData = Serialize.Server.Instance.SerializeAllNodesToString();

                string directory = path.GetBaseDir();
                DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(directory));

                using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
                if (file == null)
                {
                    Log.Error($"Failed to open file for writing: {path}. Error: {FileAccess.GetOpenError()}",
                        ELogCategory.SaveLoad);
                    return;
                }
                file.StoreString(jsonData);
                
                Log.Debug("Game saved successfully.", ELogCategory.SaveLoad);
            }
            catch (Exception ex)
            {
                Log.Error($"Exception during SaveGame: {ex.Message}\nStackTrace: {ex.StackTrace}",
                    ELogCategory.SaveLoad);
            }
        }

        public static void LoadGame(string path)
        {
            if (Serialize.Server.Instance == null)
            {
                Log.Error("Cannot load game: Serialize.Server.Instance is null.",
                    ELogCategory.SaveLoad);
                return;
            }

            Log.Debug($"Attempting to load game from: {path}", ELogCategory.SaveLoad);
            
            if (FileAccess.FileExists(path))
            {
                try
                {
                    using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
                    if (file == null)
                    {
                        Log.Error($"Failed to open file for reading: {path}. Error: {FileAccess.GetOpenError()}",
                            ELogCategory.SaveLoad);
                        return;
                    }
                    string jsonData = file.GetAsText();

                    if (string.IsNullOrWhiteSpace(jsonData))
                    {
                        Log.Warn($"Loaded save file '{path}' is empty or whitespace. Skipping deserialization.",
                            ELogCategory.SaveLoad);
                        return;
                    }

                    Serialize.Server.Instance.DeserializeAllNodesFromString(jsonData);
                    Log.Info("Game loaded successfully.", ELogCategory.SaveLoad);
                }
                catch (Exception ex)
                {
                    Log.Error($"Exception during LoadGame from '{path}': {ex.Message}\nStackTrace: {ex.StackTrace}",
                        ELogCategory.SaveLoad);
                }
            }
            else
            {
                Log.Warn($"No save file found at '{path}'. Starting with default state.", ELogCategory.SaveLoad);
            }
        }
        #endregion
    }
}