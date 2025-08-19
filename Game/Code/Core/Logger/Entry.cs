namespace Game.Core.Logger
{
    public readonly struct Entry(
        ELogLevel level,
        ELogCategory category,
        string time,
        string message,
        string callerNamespace,
        string callerClassName,
        string callerMemberName,
        string callerFilePath,
        int callerLineNumber,
        int threadId
    )
    {
        #region PUBLIC FIELDS
        public readonly ELogLevel Level = level;
        public readonly ELogCategory Category = category;
        public readonly string Time = time;
        public readonly string Message = message;
        public readonly string CallerNamespace = callerNamespace;
        public readonly string CallerClassName = callerClassName;
        public readonly string CallerMemberName = callerMemberName;
        public readonly string CallerFilePath = callerFilePath;
        public readonly int CallerLineNumber = callerLineNumber;
        public readonly int ThreadId = threadId;
        #endregion
    }
}