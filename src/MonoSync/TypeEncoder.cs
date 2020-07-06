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
    public class TypeTable
    {
        public static class ReservedIdentifiers
        {
            // 0 is reserved for null
            public const int ArrayIdentifier = 1;
        }

        private static Dictionary<string,Type> _serializableTypeLookupCache;
        private static Dictionary<string, Type> GetSerializableTypeLookup()
        {
            if (_serializableTypeLookupCache != null)
                return _serializableTypeLookupCache;

            var types = new List<Type>();
            types.Add(typeof(bool));
            types.Add(typeof(byte));
            types.Add(typeof(sbyte));
            types.Add(typeof(char));
            types.Add(typeof(decimal));
            types.Add(typeof(double));
            types.Add(typeof(float));
            types.Add(typeof(int));
            types.Add(typeof(uint));
            types.Add(typeof(long));
            types.Add(typeof(ulong));
            types.Add(typeof(short));
            types.Add(typeof(ushort));
            types.Add(typeof(string));
            types.Add(typeof(Guid));
            
            //Register SyncTypes
            types.Add(typeof(ObservableCollection<>));
            types.Add(typeof(ObservableDictionary<,>));
            types.Add(typeof(ObservableHashSet<>));

            IEnumerable<Type> typesWithSerializableAttribute =
                from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetTypes()
                let attributes = type.GetCustomAttributes(typeof(SerializableAttribute), true)
                where attributes.Length > 0
                select type;

            foreach (Type type in typesWithSerializableAttribute)
            {
                types.Add(type);
            }

            return _serializableTypeLookupCache = types.ToDictionary(type => type.AssemblyQualifiedName);
        }

        /// <summary>
        /// 0-8 are reserved
        /// </summary>
        private const int StartIndex = 8;

        private readonly Dictionary<Type, int> _idLookup = new Dictionary<Type, int>();
        private readonly Dictionary<int, Type> _typeLookup = new Dictionary<int, Type>();
        private int _typeIndex = StartIndex;

        public TypeTable()
        {
            var serializableTypes = GetSerializableTypeLookup();
            foreach (Type type in serializableTypes.Values)
            {
                RegisterType(type);
            }
        }

        public TypeTable(ExtendedBinaryReader reader)
        {
            Dictionary<string, Type> serializableTypeLookup = GetSerializableTypeLookup();

            int count = reader.Read7BitEncodedInt();
            for (int i = 0; i < count; i++)
            {
                string assemblyQualifiedName = reader.ReadString();
                if (serializableTypeLookup.TryGetValue(assemblyQualifiedName, out Type serializableType))
                {
                    RegisterType(serializableType);
                }
                else
                {
                    throw new TypeNotSerializableException(assemblyQualifiedName);
                }
            }
        }

        public void Serialize(ExtendedBinaryWriter writer)
        {
            writer.Write7BitEncodedInt(_typeLookup.Count);
            foreach (var valuePair in _typeLookup)
            {
                writer.Write(valuePair.Value.AssemblyQualifiedName);
            }
        }

        public void RegisterType(Type type)
        {
            int index = _typeIndex++;
            if (type.IsGenericType && type.IsGenericTypeDefinition == false)
            {
                throw new ArgumentException($"{nameof(type)} must be a GenericTypeDefinition");
            }

            if (_typeLookup.TryGetValue(index, out Type registeredType))
            {
                throw new IdentifierAlreadyRegisteredException(index, registeredType);
            }

            if (_idLookup.ContainsKey(type))
            {
                throw new TypeAlreadyRegisteredException(type);
            }

            _idLookup.Add(type, index);
            _typeLookup.Add(index, type);
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
    }

    public class TypeEncoder : ITypeEncoder
    {
        private readonly TypeTable _typeTable;
        private readonly Dictionary<Type, byte[]> _typeEncodingCache = new Dictionary<Type, byte[]>();

        public TypeEncoder(TypeTable typeTable)
        {
            _typeTable = typeTable;
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

            if (typeIdentifier == TypeTable.ReservedIdentifiers.ArrayIdentifier)
            {
                var arrayRank = typeIdentifiers.Dequeue();
                Type arrayType = arrayRank == 1
                    ? DecodeTypeRecursive(typeIdentifiers).MakeArrayType()
                    : DecodeTypeRecursive(typeIdentifiers).MakeArrayType(arrayRank);
                return arrayType;
            }

            Type type = _typeTable.GetTypeById(typeIdentifier);
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
                output.Enqueue(TypeTable.ReservedIdentifiers.ArrayIdentifier);

                // Enqueue Array rank/dimensions
                var arrayRank = type.GetArrayRank();
                output.Enqueue(arrayRank);

                // Enqueue elementType
                EncodeTypeRecursive(output, type.GetElementType());
            }
            

            else if (type.IsGenericType)
            {
                int identifier = _typeTable.GetIdByType(type.GetGenericTypeDefinition());
                output.Enqueue(identifier);
                
                for (var index = 0; index < type.GenericTypeArguments.Length; index++)
                {
                    Type typeArgument = type.GenericTypeArguments[index];
                    EncodeTypeRecursive(output, typeArgument);
                }
            }
            else
            {
                int identifier = _typeTable.GetIdByType(type);
                output.Enqueue(identifier);
            }
        }
    }
}