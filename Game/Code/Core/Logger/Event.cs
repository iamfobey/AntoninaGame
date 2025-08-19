namespace Game.Core.Logger
{
    public readonly struct Event(
        string classNamespace,
        string className,
        string memberName,
        string filePath,
        int lineNumber
    )
    {
        #region PUBLIC FIELDS
        public readonly string ClassNamespace = classNamespace ?? "N/A (Event)";
        public readonly string ClassName = className ?? "N/A (Event)";
        public readonly string MemberName = memberName ?? "N/A (Event)";
        public readonly string FilePath = filePath ?? "N/A (Event)";
        public readonly int? LineNumber = lineNumber;
        #endregion
    }
}