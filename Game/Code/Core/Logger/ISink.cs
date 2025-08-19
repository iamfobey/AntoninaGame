namespace Game.Core.Logger.Sinks
{
    public interface ISink
    {
        #region PUBLIC METHODS
        public virtual void Log(Entry entry)
        {

        }
        #endregion
    }
}