using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Policy;
using MonoSync.Collections;
using MonoSync.Exceptions;
using MonoSync.Utils;

namespace MonoSync
{
    internal class TypeEncoder
    {
        private readonly Dictionary<Type, byte[]> _typeEncodingCache = new Dictionary<Type, byte[]>();
        private const int ReservedIndexOffset = 2;

        /// <summary>
        /// All custom types
        /// </summary>
        private readonly List<Type> _allCustomTypes = new List<Type>();

        /// <summary>
        /// Custom types that are not synchronized yet
        /// </summary>
        private readonly List<Type> _addedCustomTypes = new List<Type>();

        private static class ReservedIdentifiers
        {
            // 0 is reserved for null
            public const int ArrayIdentifier = 1;
        }

        private readonly Dictionary<Type, int> _idLookup = new Dictionary<Type, int>();
        private readonly Dictionary<int, Type> _typeLookup = new Dictionary<int, Type>();

        public TypeEncoder()
        {
            var types = new[]
            {
                typeof(bool), typeof(byte), typeof(sbyte), typeof(char),
                typeof(decimal),
                typeof(double),
                typeof(float),
                typeof(int),
                typeof(uint),
                typeof(long),
                typeof(ulong),
                typeof(short),
                typeof(ushort),
                typeof(string),
                typeof(Guid),
                typeof(ObservableCollection<>),
                typeof(ObservableDictionary<,>),
                typeof(ObservableHashSet<>)
            };

            foreach (Type type in types)
            {
                RegisterDefaultType(type);
            }
        }

        public void WriteAllTypes(ExtendedBinaryWriter writer)
        {
            writer.Write7BitEncodedInt(_allCustomTypes.Count);
            foreach (Type type in _allCustomTypes)
            {
                writer.Write(type.AssemblyQualifiedName);
            }
        }

        public void WriteAddedTypes(ExtendedBinaryWriter writer)
        {
            writer.Write7BitEncodedInt(_addedCustomTypes.Count);
            foreach (var type in _addedCustomTypes)
            {
                writer.Write(type.AssemblyQualifiedName);
            }
        }

        public void Read(ExtendedBinaryReader reader)
        {
            int count = reader.Read7BitEncodedInt();
            for (int i = 0; i < count; i++)
            {
                string assemblyQualifiedName = reader.ReadString();
                var type = Type.GetType(assemblyQualifiedName);
                RegisterDefaultType(type);
            }
        }

        /// <summary>
        /// Registers the type
        /// </summary>
        /// <param name="type"></param>
        public bool RegisterType(Type type)
        {
            if (type.IsGenericType)
            {
                // Recursive generic argument registration
                foreach (Type genericArgument in type.GetGenericArguments())
                {
                    RegisterType(genericArgument);
                }
                type = type.GetGenericTypeDefinition();
            }

            if (RegisterDefaultType(type))
            {
                _allCustomTypes.Add(type);
                _addedCustomTypes.Add(type);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Registers the type without serializing it
        /// </summary>
        /// <param name="type"></param>
        private bool RegisterDefaultType(Type type)
        {
            int index = _typeLookup.Count + ReservedIndexOffset;
            if (type.IsGenericType && type.IsGenericTypeDefinition == false)
            {
                throw new ArgumentException($"{nameof(type)} must be a GenericTypeDefinition");
            }
            if (_idLookup.ContainsKey(type))
            {
                return false;
            }
            _idLookup.Add(type, index);
            _typeLookup.Add(index, type);
            return true;
        }

        public void RegisterType<T>()
        {
            RegisterType(typeof(T));
        }

        public Type GetTypeById(int typeId)
        {
            if (_typeLookup.TryGetValue(typeId, out Type type))
            {
                return type;
            }
            throw new TypeIdNotRegisteredException(typeId);
        }

        public int GetIdByType(Type type)
        {
            if (_idLookup.TryGetValue(type, out int id))
            {
                return id;
            }
            throw new TypeNotSerializableException(type.AssemblyQualifiedName);
        }


        public Type ReadType(ExtendedBinaryReader reader)
        {
            var count = reader.Read7BitEncodedInt();
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
            if (_typeEncodingCache.TryGetValue(type, out byte[] encoding) == false)
            {
                using var encodingMemoryStream = new MemoryStream();
                using var encodingWriter = new ExtendedBinaryWriter(encodingMemoryStream);
                var output = new Queue<int>();
                EncodeTypeRecursive(output, type);

                encodingWriter.Write7BitEncodedInt(output.Count);

                foreach (var identifier in output)
                {
                    encodingWriter.Write7BitEncodedInt(identifier);
                }

                encoding = encodingMemoryStream.ToArray();
                _typeEncodingCache.Add(type, encoding);
            }

            writer.Write(encoding);
        }

        private Type DecodeTypeRecursive(Queue<int> typeIdentifiers)
        {
            var typeIdentifier = typeIdentifiers.Dequeue();

            if (typeIdentifier == ReservedIdentifiers.ArrayIdentifier)
            {
                var arrayRank = typeIdentifiers.Dequeue();
                Type arrayType = arrayRank == 1
                    ? DecodeTypeRecursive(typeIdentifiers).MakeArrayType()
                    : DecodeTypeRecursive(typeIdentifiers).MakeArrayType(arrayRank);
                return arrayType;
            }

            Type type = GetTypeById(typeIdentifier);
            if (type.IsGenericTypeDefinition)
            {
                var typeCount = type.GetGenericArguments().Length;
                var typeArgs = new Type[typeCount];
                for (var i = 0; i < typeCount; i++)
                {
                    typeArgs[i] = DecodeTypeRecursive(typeIdentifiers);
                }
                return type.MakeGenericType(typeArgs);
            }
            return type;
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
                var arrayRank = type.GetArrayRank();
                output.Enqueue(arrayRank);

                // Enqueue elementType
                EncodeTypeRecursive(output, type.GetElementType());
            }
            

            else if (type.IsGenericType)
            {
                int identifier = GetIdByType(type.GetGenericTypeDefinition());
                output.Enqueue(identifier);
                
                for (var index = 0; index < type.GenericTypeArguments.Length; index++)
                {
                    Type typeArgument = type.GenericTypeArguments[index];
                    EncodeTypeRecursive(output, typeArgument);
                }
            }
            else
            {
                int identifier = GetIdByType(type);
                output.Enqueue(identifier);
            }
        }

        public void EndWrite()
        {
            _addedCustomTypes.Clear();
        }
    }
}