using System;
using System.Collections.Generic;
using System.Text;
using MonoSync.Collections;
using Xunit;

namespace MonoSync.Test.Collections
{
    public class ObservableDictionaryTests
    {
        [Fact]
        public void MassUpdate_PreviousMassUpdateNotDisposed_ThrowsInvalidOperationException()
        {
            var dictionaryUnderTest = new ObservableDictionary<int, int>();
            dictionaryUnderTest.BeginMassUpdate();
            Assert.Throws<InvalidOperationException>(() => { dictionaryUnderTest.BeginMassUpdate(); });
        }
    }
}
