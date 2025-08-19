using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Game.Core.Logger.Sinks;
using Godot;

namespace Game.Core.Logger
{
    public partial class Server : Node
    {
        #region PUBLIC METHODS
        public override void _EnterTreeGame()
        {
            base._EnterTreeGame();

            Instance = this;
        }

        public void Log(
            ELogLevel level, ELogCategory category, string message,
            Event? logEvent = null,
            [CallerMemberName] string directLoggerCallerMemberName = "",
            [CallerFilePath] string directLoggerCallerFilePath = "",
            [CallerLineNumber] int directLoggerCallerLineNumber = 0)
        {
            if (!IsEnabled || level < MinimumLogLevel)
            {
                return;
            }

            string finalNamespace;
            string finalClassName;
            string finalMemberName;
            string finalFilePath;
            int finalLineNumber;

            if (logEvent?.MemberName != null && logEvent.Value is { FilePath: not null, LineNumber: not null })
            {
                finalNamespace = logEvent.Value.ClassNamespace;
                finalClassName = logEvent.Value.ClassName;
                finalMemberName = logEvent.Value.MemberName;
                finalFilePath = UseShortFilePath ? Path.GetFileName(logEvent.Value.FilePath) : logEvent.Value.FilePath;
                finalLineNumber = logEvent.Value.LineNumber.Value;
            }
            else
            {
                var frame = new StackFrame(2, false);
                var method = frame.GetMethod();
                var declaringType = method?.DeclaringType;

                finalNamespace = declaringType?.Namespace ?? "N/A (Direct)";
                finalClassName = declaringType?.Name ?? "N/A (Direct)";

                if (finalClassName.Contains("<") && finalClassName.Contains(">"))
                {
                    if (declaringType?.DeclaringType != null)
                    {
                        finalClassName = declaringType.DeclaringType.Name;
                    }
                }
                finalMemberName = directLoggerCallerMemberName;
                finalFilePath = UseShortFilePath ? Path.GetFileName(directLoggerCallerFilePath) : directLoggerCallerFilePath;
                finalLineNumber = directLoggerCallerLineNumber;
            }

            var logEntry = new Entry(
                level, category, DateTime.UtcNow.ToString(TimeFormat), message,
                finalNamespace, finalClassName, finalMemberName, finalFilePath, finalLineNumber,
                Thread.CurrentThread.ManagedThreadId
            );

            ProcessLogEntry(logEntry);
        }
        #endregion

        #region PRIVATE METHODS
        private void ProcessLogEntry(Entry entry)
        {
            foreach (var sink in Sinks)
            {
                sink.Log(entry);
            }
        }
        #endregion

        #region STATIC FIELDS
        public static Server Instance { get; private set; }
        #endregion

        #region PUBLIC FIELDS
        public bool IsEnabled = true;
        public ELogLevel MinimumLogLevel = ELogLevel.Debug;
        public string TimeFormat = "HH:mm:ss.fff";
        public bool UseShortFilePath = true;
        public List<ISink> Sinks = [new Sinks.Godot()];
        #endregion
    }
}