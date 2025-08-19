using System.Text;
using Godot;

namespace Game.Core.Logger.Sinks
{
    public sealed class Godot : ISink
    {
        #region STATIC METHODS
        public static string FormatLogEntry(Entry entry)
        {
            var sb = new StringBuilder();

            sb.Append($"[{entry.Time}] ");
            sb.Append($"[{entry.Level.ToString().ToUpper()}] ");
            sb.Append($"[{entry.Category}] ");
            sb.Append(
                $"[{entry.CallerFilePath}::{entry.CallerNamespace}::{entry.CallerClassName}::{entry.CallerMemberName}::{entry.CallerLineNumber}] ");
            sb.Append($"[TID:{entry.ThreadId}] ");
            sb.Append(entry.Message);
            return sb.ToString();
        }
        #endregion

        #region PUBLIC METHODS
        public void Log(Entry entry)
        {
            string formattedMessage = FormatLogEntry(entry);
            string bbCodeFormattedMessage = formattedMessage;

            switch (entry.Level)
            {
                case ELogLevel.Debug:
                    bbCodeFormattedMessage = $"[color=gray]{formattedMessage}[/color]";
                    break;
                case ELogLevel.Info:
                    bbCodeFormattedMessage = $"[color=lightgray]{formattedMessage}[/color]";
                    break;
                case ELogLevel.Warning:
                    #if DEBUG || TOOLS
                    GD.PushWarning(formattedMessage);
                    bbCodeFormattedMessage = $"[color=yellow]{formattedMessage}[/color]";
                    #endif
                    break;
                case ELogLevel.Error:
                    #if DEBUG || TOOLS
                    GD.PushError(formattedMessage);
                    bbCodeFormattedMessage = $"[color=red]{formattedMessage}[/color]";
                    #endif
                    break;
                case ELogLevel.Critical:
                    #if DEBUG || TOOLS
                    GD.PushError(formattedMessage);
                    bbCodeFormattedMessage = $"[color=red]{formattedMessage}[/color]";
                    #endif
                    break;
                default:
                    GD.PushError($"[CRITICAL] [Logger] Unknown log level encountered: {entry.Level}");
                    break;
            }

            #if DEBUG || TOOLS
            GD.PrintRich(bbCodeFormattedMessage);
            #endif
        }
        #endregion
    }
}