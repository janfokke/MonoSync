using Newtonsoft.Json;
using Xunit;

namespace MonoSync.Test
{
    internal static class AssertExtension
    {
        public static void AssertCloneEqual(object expected, object actual)
        {
            Assert.Equal(
                JsonConvert.SerializeObject(expected),
                JsonConvert.SerializeObject(actual));
        }
    }
}