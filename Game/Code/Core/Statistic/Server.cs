using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;

namespace Game.Core.Statistic
{
    public partial class Server : Node
    {
        #region PUBLIC METHODS
        public override void _EnterTreeGame()
        {
            DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath("user://Saved/"));
            _jsonFilePath = ProjectSettings.GlobalizePath("user://Saved/statistics.json");

            LoadSessions();

            var newSession = new SessionData
            {
                StartTime = DateTime.Now,
                DurationSeconds = 0
            };

            _sessions.Add(newSession);
        }

        public override void _ProcessGame(double delta)
        {
            _sessionTime += delta;
        }

        public override void _ExitTreeGame()
        {
            if (_sessions.Count > 0)
                _sessions[^1].DurationSeconds = Math.Round(_sessionTime, 2);

            SaveSessions();
        }
        #endregion

        #region PRIVATE METHODS
        private void LoadSessions()
        {
            if (File.Exists(_jsonFilePath))
            {
                string jsonText = File.ReadAllText(_jsonFilePath);
                _sessions = JsonSerializer.Deserialize<List<SessionData>>(jsonText) ?? [];
            }
        }

        private void SaveSessions()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonText = JsonSerializer.Serialize(_sessions, options);
            File.WriteAllText(_jsonFilePath, jsonText);
        }
        #endregion

        #region PRIVATE FIELDS
        private double _sessionTime = 0.0;
        private string _jsonFilePath;
        private List<SessionData> _sessions = [];
        #endregion

        private class SessionData
        {
            #region PUBLIC FIELDS
            public DateTime StartTime { get; set; }
            public double DurationSeconds { get; set; }
            #endregion
        }
    }
}