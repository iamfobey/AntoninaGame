using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Game.Core.Logger
{
    public enum ELogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }

    public enum ELogCategory
    {
        // Core
        System,
        Navigation,
        Physics,
        Rendering,
        Audio,
        Network,
        UI,
        Input,
        SaveLoad,
        Animation,
        FSM,
        Configuration,
        Memory,
        Threading,
        Serialization,
        Localization,
        Ads,
        Analytics,

        // Logic
        GameLogic,
        Player,
        AI,

        // Other
        Performance,
        Editor,
        ThirdParty,
        Utils,

        Temp
    }

    public static class Log
    {
        #region STATIC METHODS
        [Conditional("LOGGER_DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Debug(string message, ELogCategory category = ELogCategory.Temp, Event? logEvent = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            Server.Instance.Log(ELogLevel.Debug, category, message, logEvent, callerMemberName, callerFilePath, callerLineNumber);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Info(string message, ELogCategory category = ELogCategory.Temp, Event? logEvent = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            Server.Instance.Log(ELogLevel.Info, category, message, logEvent, callerMemberName, callerFilePath, callerLineNumber);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warn(string message, ELogCategory category = ELogCategory.Temp, Event? logEvent = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            Server.Instance.Log(ELogLevel.Warning, category, message, logEvent, callerMemberName, callerFilePath, callerLineNumber);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(string message, ELogCategory category = ELogCategory.Temp, Event? logEvent = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            Server.Instance.Log(ELogLevel.Error, category, message, logEvent, callerMemberName, callerFilePath, callerLineNumber);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Critical(string message, ELogCategory category = ELogCategory.Temp, Event? logEvent = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            Server.Instance.Log(ELogLevel.Critical, category, message, logEvent, callerMemberName, callerFilePath, callerLineNumber);
        }
        #endregion
    }
}