using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using MonoSync.Attributes;
using MonoSync.Exceptions;
using MonoSync.SyncSource;
using MonoSync.Utils;

namespace MonoSync.SyncTarget.SyncTargetObjects
{
    public class NotifyPropertyChangedSyncTarget : SyncTarget
    {
        private readonly List<SyncTargetProperty> _referenceProperties = new List<SyncTargetProperty>();
        private readonly SyncTargetRoot _syncTargetRoot;
        private readonly SyncTargetProperty[] _targetPropertyByIndexLookup;

        private readonly Dictionary<string, SyncTargetProperty> _targetPropertyByNameLookup =
            new Dictionary<string, SyncTargetProperty>();

        private bool _constructing;
        private Action<List<object>> _constructor;

        public NotifyPropertyChangedSyncTarget(int referenceId, Type baseType, ExtendedBinaryReader reader,
            SyncTargetRoot syncTargetRoot,
            IFieldSerializerResolver fieldDeserializerResolver) : base(referenceId)
        {
            _syncTargetRoot = syncTargetRoot;

            ConstructorInfo attributeMarkedConstructor = GetAttributeMarkedConstructor(baseType);
            bool attributeMarkedConstructorPresent = attributeMarkedConstructor != null;
            base.BaseObject = attributeMarkedConstructorPresent
                ? FormatterServices.GetUninitializedObject(baseType)
                : Activator.CreateInstance(baseType);
            BaseObject.PropertyChanged += TargetOnPropertyChanged;

            (PropertyInfo info, SyncAttribute attibute)[] syncPropertiesInfo = GetProperties();

            _targetPropertyByIndexLookup = new SyncTargetProperty[syncPropertiesInfo.Length];
            for (var syncPropertyIndex = 0; syncPropertyIndex < syncPropertiesInfo.Length; syncPropertyIndex++)
            {
                (PropertyInfo propertyInfo, SyncAttribute attribute) = syncPropertiesInfo[syncPropertyIndex];
                SyncTargetProperty syncTargetProperty =
                    CreateSyncTargetProperty(fieldDeserializerResolver, propertyInfo, syncPropertyIndex);
                _targetPropertyByNameLookup[propertyInfo.Name] = syncTargetProperty;
                _targetPropertyByIndexLookup[syncPropertyIndex] = syncTargetProperty;
                // Reference properties getters will be used for reference collection.
                if (propertyInfo.PropertyType.IsValueType == false)
                {
                    _referenceProperties.Add(syncTargetProperty);
                }
            }

            Read(reader);
            _constructor = constructionPath =>
            {
                if (attributeMarkedConstructorPresent)
                {
                    ConstructFromAttributeMarkedConstructor(attributeMarkedConstructor, syncPropertiesInfo,
                        constructionPath);
                }
                else
                {
                    InitializeProperties(syncPropertiesInfo);
                }
            };
            _syncTargetRoot.EndRead += SyncTargetRootOnEndRead;
        }

        private new INotifyPropertyChanged BaseObject => (INotifyPropertyChanged) base.BaseObject;

        private SyncTargetProperty CreateSyncTargetProperty(IFieldSerializerResolver fieldDeserializerResolver,
            PropertyInfo propertyInfo, int syncPropertyIndex)
        {
            Func<object> getter = PropertyInfoHelper.CreateGetterDelegate(propertyInfo, BaseObject);
            Action<object> setter = PropertyInfoHelper.CreateSetterDelegate(propertyInfo, BaseObject);
            IFieldSerializer fieldSerializer =
                fieldDeserializerResolver.FindMatchingSerializer(propertyInfo.PropertyType);
            var syncTargetProperty = new SyncTargetProperty(syncPropertyIndex, setter, getter,
                _syncTargetRoot, fieldSerializer);
            return syncTargetProperty;
        }

        private void InitializeProperties((PropertyInfo info, SyncAttribute attibute)[] syncProperties)
        {
            for (var index = 0;
                index < _targetPropertyByIndexLookup.Length;
                index++)
            {
                SyncTargetProperty syncTargetProperty = _targetPropertyByIndexLookup[index];
                syncTargetProperty.Property = syncTargetProperty.SynchronizedValue;
                syncTargetProperty.SynchronizationBehaviour = syncProperties[index].attibute.SynchronizationBehaviour;
            }
        }

        private void ConstructFromAttributeMarkedConstructor(ConstructorInfo constructor,
            (PropertyInfo info, SyncAttribute attibute)[] syncProperties, List<object> constructionPath)
        {
            HashSet<SyncTargetProperty> parameterProperties = GetPropertiesFromConstructorParameters(constructor);

            foreach (SyncTargetProperty syncTargetProperty in parameterProperties)
            {
                ConstructProperty(syncTargetProperty, constructionPath);
            }

            constructor.Invoke(BaseObject, parameterProperties.Select(x => x.SynchronizedValue).ToArray());

            // Initialize properties that where not given as constructor parameters
            for (var index = 0; index < _targetPropertyByIndexLookup.Length; index++)
            {
                SyncTargetProperty syncTargetProperty = _targetPropertyByIndexLookup[index];
                // Skip properties that are initialized from constructor
                if (parameterProperties.Contains(syncTargetProperty))
                {
                    continue;
                }

                syncTargetProperty.Property = syncTargetProperty.SynchronizedValue;
                syncTargetProperty.SynchronizationBehaviour = syncProperties[index].attibute.SynchronizationBehaviour;
            }
        }

        private void ConstructProperty(SyncTargetProperty syncTargetProperty, List<object> path)
        {
            object synchronizedValue = syncTargetProperty.SynchronizedValue;
            if (synchronizedValue != null)
            {
                Type type = synchronizedValue.GetType();
                if (type.IsValueType == false)
                {
                    SyncTarget target = _syncTargetRoot.ReferencePool.GetSyncObject(synchronizedValue);
                    if (target is NotifyPropertyChangedSyncTarget notifyPropertyChangedSyncTarget)
                    {
                        notifyPropertyChangedSyncTarget.Construct(path);
                    }
                }
            }
        }

        private void Construct(List<object> path)
        {
            if (_constructor == null)
            {
                return;
            }

            path.Add(BaseObject);

            if (_constructing == false)
            {
                _constructing = true;
                _constructor(path);
            }
            else
            {
                throw new ConstructorReferenceCycleException(path);
            }

            _constructor = null;
        }

        private HashSet<SyncTargetProperty> GetPropertiesFromConstructorParameters(ConstructorInfo constructor)
        {
            return new HashSet<SyncTargetProperty>(GetPropertyNameFromConstructorParameter(constructor).Select(
                propertyName =>
                {
                    if (_targetPropertyByNameLookup.TryGetValue(propertyName, out SyncTargetProperty value))
                    {
                        return value;
                    }

                    throw new SyncTargetPropertyNotFoundException(propertyName);
                }));
        }

        private static IEnumerable<string> GetPropertyNameFromConstructorParameter(
            ConstructorInfo attributeMarkedConstructor)
        {
            return attributeMarkedConstructor.GetParameters().Select(parameter =>
            {
                string name = parameter.GetCustomAttributes()
                    .OfType<SyncConstructorParameterAttribute>()
                    .FirstOrDefault()?.PropertyName;

                return name ?? CapitalizeFirstLetter(parameter.Name);
            });
        }

        private static string CapitalizeFirstLetter(string input)
        {
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        private (PropertyInfo info, SyncAttribute attibute)[] GetProperties()
        {
            PropertyInfo[] properties = BaseObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            (PropertyInfo info, SyncAttribute attibute)[] syncProperties =
                PropertyInfoHelper.GetSyncProperties(properties).ToArray();
            return syncProperties;
        }

        private static ConstructorInfo GetAttributeMarkedConstructor(Type baseType)
        {
            return baseType.GetConstructors()
                .FirstOrDefault(constructorInfo =>
                    constructorInfo.GetCustomAttributes()
                        .Any(a => a is SyncConstructorAttribute));
        }

        private void SyncTargetRootOnEndRead(object sender, EventArgs e)
        {
            Construct(new List<object>());
            _syncTargetRoot.EndRead -= SyncTargetRootOnEndRead;
        }

        internal SyncTargetProperty GetSyncTargetProperty(string propertyName)
        {
            if (_targetPropertyByNameLookup.TryGetValue(propertyName, out SyncTargetProperty property))
            {
                return property;
            }

            throw new SyncTargetPropertyNotFoundException(propertyName);
        }

        private void TargetOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_targetPropertyByNameLookup.TryGetValue(e.PropertyName, out SyncTargetProperty syncTargetProperty)
            )
            {
                return;
            }

            syncTargetProperty.NotifyChanged();
        }

        public override void Dispose()
        {
            BaseObject.PropertyChanged -= TargetOnPropertyChanged;
            foreach (SyncTargetProperty syncTargetProperty in _targetPropertyByIndexLookup)
            {
                syncTargetProperty.Dispose();
            }
        }

        public override IEnumerable<object> GetReferences()
        {
            return _referenceProperties.Select(x => x.Property);
        }

        public sealed override void Read(ExtendedBinaryReader reader)
        {
            BitArray changedProperties = reader.ReadBitArray(_targetPropertyByIndexLookup.Length);
            for (var index = 0; index < _targetPropertyByIndexLookup.Length; index++)
            {
                SyncTargetProperty syncTargetProperty = _targetPropertyByIndexLookup[index];

                if (changedProperties[index])
                {
                    syncTargetProperty.ReadChanges(reader);
                }
            }
        }
    }
}