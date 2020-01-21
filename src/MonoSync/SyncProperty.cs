namespace MonoSync
{
    public abstract class SyncProperty
    {
        protected SyncProperty(int index)
        {
            Index = index;
        }

        internal int Index { get; }
    }
}