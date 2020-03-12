using System;
using System.Collections.ObjectModel;
using System.IO;
using MonoSync.Collections;
using MonoSync.Exceptions;
using MonoSync.Utils;
using Xunit;
using static MonoSync.TypeEncoder.ReservedIdentifiers;

namespace MonoSync.Test.TypeEncoding
{
    public class TypeEncodingTests
    {
        [Theory]
        // Simple types
        [InlineData(typeof(bool))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(char))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(double))]
        [InlineData(typeof(float))]
        [InlineData(typeof(int))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(long))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(short))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(string))]
        [InlineData(typeof(Guid))]

        // Generics
        [InlineData(typeof(ObservableCollection<int>))]
        [InlineData(typeof(ObservableDictionary<int, string>))]

        // Nested Generics
        [InlineData(
            typeof(ObservableDictionary<ObservableCollection<ObservableDictionary<int, string>>,
                ObservableCollection<float>>))]

        //Z Arrays
        [InlineData(typeof(int[,]))]
        [InlineData(typeof(uint[,,]))]

        // Jagged Arrays
        [InlineData(typeof(long[]))]
        [InlineData(typeof(ulong[][]))]
        [InlineData(typeof(short[][][]))]
        public void EncodingAndDecodingType_ResultsInSameType(Type expectedType)
        {
            var typeEncoder = new TypeEncoder();

            using var memoryStream = new MemoryStream();
            using var writer = new ExtendedBinaryWriter(memoryStream);
            typeEncoder.WriteType(expectedType, writer);

            memoryStream.Position = 0;
            using var reader = new ExtendedBinaryReader(memoryStream);

            Type actualType = typeEncoder.ReadType(reader);
            Assert.Equal(expectedType, actualType);
        }

        [Fact]
        public void RegisteringType_WithAnIdentifierThatIsAlreadyUsed_ThrowsIdentifierAlreadyRegisteredException()
        {
            var typeEncoder = new TypeEncoder();
            Assert.Throws<IdentifierAlreadyRegisteredException>(() =>
            {
                typeEncoder.RegisterType<UniqueType>(Int32Identifier);
            });
        }

        [Fact]
        public void RegisteringType_ThatIsAlreadyRegistered_ThrowsTypeAlreadyRegisteredException()
        {
            var typeEncoder = new TypeEncoder();
            Assert.Throws<TypeAlreadyRegisteredException>(() =>
            {
                typeEncoder.RegisterType<int>(StartingIndexNonReservedTypes);
            });
        }
    }

    internal class UniqueType
    {
    }
}