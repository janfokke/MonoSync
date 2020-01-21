using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [AddINotifyPropertyChangedInterface]
    internal class ReferencingCircleHelper
    {
        [Sync] public ReferencingCircleHelper Other { get; set; }
    }
}