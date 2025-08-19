namespace Game.Core.Serialize
{
    public interface IGameSerializeAware
    {
        #region PUBLIC METHODS
        void _OnBeforeSerialize();
        void _OnSerialize();

        void _OnBeforeDeserialize();
        void _OnDeserialize();
        #endregion
    }
}