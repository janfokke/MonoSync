using MonoSync.Attributes;
using MonoSync.Collections;
using PropertyChanged;

namespace MonoSync.Test.TestObjects
{
    [AddINotifyPropertyChangedInterface]
    internal class SynchronizeConstructorMock
    {
        private ObservableDictionary<int, int> _dictionary;

        public SynchronizeConstructorMock() : this(new ObservableDictionary<int, int>())
        {
        }

        [SyncConstructor]
        public SynchronizeConstructorMock(ObservableDictionary<int, int> dictionary)
        {
            Dictionary = dictionary;
            SyncConstructorCalled = true;
        }

        public int DictionarySetCount { get; private set; }

        [Sync]
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