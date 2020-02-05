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

    [AddINotifyPropertyChangedInterface]
    public class OnSynchronizedAttributeMarkedMethodMock
    {
        [Sync]
        public int intProperty { get; set; }

        public int intPropertyWhenSynchronizedMethodWasCalled { get; private set; }

        [OnSynchronized]
        public void OnSynchronized()
        {
            intPropertyWhenSynchronizedMethodWasCalled = intProperty;
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class GetterOnlyMock
    {
        [Sync]
        public int IntProperty { get; }
    }

    [AddINotifyPropertyChangedInterface]
    public class GetterOnlyConstructorMock
    {
        [SyncConstructor]
        public GetterOnlyConstructorMock(int intProperty)
        {
            IntProperty = intProperty;
        }

        [Sync]
        public int IntProperty { get; }
    }

    public class OnSynchronizedAttributeMarkedMethodMockChild : OnSynchronizedAttributeMarkedMethodMock
    {

    }

    [AddINotifyPropertyChangedInterface]
    public class OnSynchronizedAttributeMarkedMethodWithParametersMock
    {
        public int intProperty { get; set; }
        public int intPropertyWhenSynchronizedMethodWasCalled { get; private set; }

        [OnSynchronized]
        public void OnSynchronized(int illegalParameter)
        {
            
        }
    }
}