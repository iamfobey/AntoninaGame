#if IMGUI
using System.Collections.Concurrent;

namespace Game.Core.Logger.Sinks
{
    public sealed class ImGuiSink : ISink
    {
        #region PUBLIC METHODS
        public void Log(Entry entry)
        {
            LogQueue.Enqueue(entry);
        }
        #endregion

        #region PUBLIC FIELDS
        public readonly ConcurrentQueue<Entry> LogQueue = new();
        #endregion
    }
}
#endif