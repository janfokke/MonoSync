using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MonoSync.Attributes;
using MonoSync.Exceptions;
using MonoSync.Utils;

namespace MonoSync.Synchronizers
{
    public class SourceMemberCollection
    {
        private readonly Dictionary<string, SyncSourceProperty> _propertiesByName;
        private readonly SyncSourceProperty[] _syncSourceProperties;

        public int Length { get; }
        public SyncSourceProperty this[int index] => _syncSourceProperties[index];

        public SourceMemberCollection(SyncSourceProperty[] syncSourceProperties)
        {
            _syncSourceProperties = syncSourceProperties;
            _propertiesByName = syncSourceProperties.ToDictionary(x => x.Name);
            Length = _syncSourceProperties.Length;
        }

        public bool TryGetPropertyByName(string propertyName, out SyncSourceProperty syncSourceProperty)
        {
            return _propertiesByName.TryGetValue(propertyName, out syncSourceProperty);
        }
    }
}