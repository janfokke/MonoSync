using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MonoSync.Collections;
using MonoSync.Exceptions;
using MonoSync.Utils;

namespace MonoSync
{
    public class TypeEncoder : ITypeEncoder
    {
        private readonly Dictionary<Type, int> _idLookup = new Dictionary<Type, int>();
        private readonly Dictionary<int, Type> _typeLookup = new Dictionary<int, Type>();

        public TypeEncoder()
        {
            //Register dotnet types
            RegisterType<bool>(ReservedIdentifiers.BooleanIdentifier);
            RegisterType<byte>(ReservedIdentifiers.ByteIdentifier);
            RegisterType<sbyte>(ReservedIdentifiers.SByteIdentifier);
            RegisterType<char>(ReservedIdentifiers.CharIdentifier);
            RegisterType<decimal>(ReservedIdentifiers.DecimalIdentifier);
            RegisterType<double>(ReservedIdentifiers.DoubleIdentifier);
            RegisterType<float>(ReservedIdentifiers.SingleIdentifier);
            RegisterType<int>(ReservedIdentifiers.Int32Identifier);
            RegisterType<uint>(ReservedIdentifiers.UInt32Identifier);
            RegisterType<long>(ReservedIdentifiers.Int64Identifier);
            RegisterType<ulong>(ReservedIdentifiers.UInt64Identifier);
            RegisterType<short>(ReservedIdentifiers.Int16Identifier);
            RegisterType<ushort>(ReservedIdentifiers.UInt16Identifier);
            RegisterType<string>(ReservedIdentifiers.StringIdentifier);

            RegisterType<Guid>(ReservedIdentifiers.Guid);

            //Register SyncTypes
            RegisterType(typeof(ObservableCollection<>), ReservedIdentifiers.ObservableCollectionIdentifier);
            RegisterType(typeof(ObservableDictionary<,>), ReservedIdentifiers.ObservableDictionaryIdentifier);
        }

        public Type ReadType(ExtendedBinaryReader reader)
        {
            int count = reader.Read7BitEncodedInt();
            var identifiers = new int[count];
            for (var i = 0; i < count; i++)
            {
                identifiers[i] = reader.Read7BitEncodedInt();
            }

            var queue = new Queue<int>(identifiers);
            return DecodeTypeRecursive(queue);
        }

        /// <summary>
        ///     Converts type into a compact integer array which can be decoded using <see cref="ReadType" />
        /// </summary>
        /// <returns></returns>
        public void WriteType(Type type, ExtendedBinaryWriter writer)
        {
            var output = new Queue<int>();
            EncodeTypeRecursive(output, type);
            writer.Write7BitEncodedInt(output.Count);
            foreach (int identifier in output)
            {
                writer.Write7BitEncodedInt(identifier);
            }
        }

        private Type DecodeTypeRecursive(Queue<int> typeIdentifiers)
        {
            int typeIdentifier = typeIdentifiers.Dequeue();

            if (typeIdentifier == ReservedIdentifiers.ArrayIdentifier)
            {
                int arrayRank = typeIdentifiers.Dequeue();
                Type arrayType = arrayRank == 1
                    ? DecodeTypeRecursive(typeIdentifiers).MakeArrayType()
                    : DecodeTypeRecursive(typeIdentifiers).MakeArrayType(arrayRank);

                return arrayType;
            }

            if (_typeLookup.TryGetValue(typeIdentifier, out Type type))
            {
                if (type.IsGenericTypeDefinition)
                {
                    int typeCount = type.GetGenericArguments().Length;
                    var typeArgs = new Type[typeCount];
                    for (var i = 0; i < typeCount; i++)
                    {
                        typeArgs[i] = DecodeTypeRecursive(typeIdentifiers);
                    }

                    return type.MakeGenericType(typeArgs);
                }

                return type;
            }

            throw new TypeIdNotRegisteredException(typeIdentifier);
        }

        /// <param name="output">The encoded type are added to this.</param>
        /// <param name="type"></param>
        private void EncodeTypeRecursive(Queue<int> output, Type type)
        {
            if (type.IsGenericTypeDefinition)
            {
                throw new ArgumentException($"{nameof(type)} is a GenericTypeDefinition");
            }

            if (type.IsArray)
            {
                // Enqueue Array indicator
                output.Enqueue(ReservedIdentifiers.ArrayIdentifier);

                // Enqueue Array rank/dimensions
                int arrayRank = type.GetArrayRank();
                output.Enqueue(arrayRank);

                // Enqueue elementType
                EncodeTypeRecursive(output, type.GetElementType());
            }

            else if (type.IsGenericType)
            {
                if (_idLookup.TryGetValue(type.GetGenericTypeDefinition(), out int identifier))
                {
                    output.Enqueue(identifier);
                }

                for (var index = 0; index < type.GenericTypeArguments.Length; index++)
                {
                    Type typeArgument = type.GenericTypeArguments[index];
                    EncodeTypeRecursive(output, typeArgument);
                }
            }
            else
            {
                if (_idLookup.TryGetValue(type, out int identifier))
                {
                    output.Enqueue(identifier);
                }
                else
                {
                    throw new TypeNotRegisteredException(type);
                }
            }
        }

        public void RegisterType(Type type, int identifier)
        {
            if (type.IsGenericType && type.IsGenericTypeDefinition == false)
            {
                throw new ArgumentException($"{nameof(type)} must be a GenericTypeDefinition");
            }

            if (_typeLookup.TryGetValue(identifier, out Type registeredType))
            {
                throw new IdentifierAlreadyRegisteredException(identifier, registeredType);
            }

            if (_idLookup.ContainsKey(type))
            {
                throw new TypeAlreadyRegisteredException(type);
            }

            _idLookup.Add(type, identifier);
            _typeLookup.Add(identifier, type);
        }

        public void RegisterType<T>(int identifier)
        {
            RegisterType(typeof(T), identifier);
        }

        public static class ReservedIdentifiers
        {
            // 0 is used for null
            public const int ArrayIdentifier = 1;

            // dotnet types
            public const int BooleanIdentifier = 2;
            public const int ByteIdentifier = 3;
            public const int SByteIdentifier = 4;
            public const int CharIdentifier = 5;
            public const int DecimalIdentifier = 6;
            public const int DoubleIdentifier = 7;
            public const int SingleIdentifier = 8;
            public const int Int32Identifier = 9;
            public const int UInt32Identifier = 10;
            public const int Int64Identifier = 11;
            public const int UInt64Identifier = 12;
            public const int Int16Identifier = 13;
            public const int UInt16Identifier = 14;
            public const int StringIdentifier = 15;

            public const int ObservableCollectionIdentifier = 16;
            public const int ObservableDictionaryIdentifier = 17;
            public const int ObservableArrayIdentifier = 19;

            public const int Guid = 18;

            public const int StartingIndexNonReservedTypes = 64;

            public static bool IsReserved(int identifier)
            {
                // types are send as 7bit encoded integers so I decided to reserve the first 6
                // bits for predefined types.
                return identifier <= 0b111111;
            }
        }
    }
}