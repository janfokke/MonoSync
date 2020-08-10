using MonoSync.Attributes;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [AddINotifyPropertyChangedInterface]
    internal class NotifyPropertyChangedReferencingCircleHelper
    {
        [Synchronize] public NotifyPropertyChangedReferencingCircleHelper Other { get; set; }
    }
}