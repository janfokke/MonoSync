using MonoSync.Attributes;
using MonoSync.Collections;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [AddINotifyPropertyChangedInterface]
    internal class NotifyPropertyChangedSynchronizeConstructorMock
    {
        private ObservableDictionary<int, int> _dictionary;

        public NotifyPropertyChangedSynchronizeConstructorMock() : this(new ObservableDictionary<int, int>())
        {
        }

        [SyncConstructor]
        public NotifyPropertyChangedSynchronizeConstructorMock(ObservableDictionary<int, int> dictionary)
        {
            Dictionary = dictionary;
            SyncConstructorCalled = true;
        }

        public int DictionarySetCount { get; private set; }

        [Synchronize]
        public ObservableDictionary<int, int> Dictionary
        {
            get => _dictionary;
            set
            {
                DictionarySetCount++;
                _dictionary = value;
            }
        }

        public bool SyncConstructorCalled { get; set; }
    }
}