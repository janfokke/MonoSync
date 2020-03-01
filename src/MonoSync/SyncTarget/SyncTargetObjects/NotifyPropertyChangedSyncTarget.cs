using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using FastMember;
using MonoSync.Attributes;
using MonoSync.Exceptions;
using MonoSync.Utils;

namespace MonoSync.SyncTargetObjects
{
    public class NotifyPropertyChangedSyncTarget : SyncTarget
    {
        private readonly SyncTargetRoot _syncTargetRoot;
        private readonly SyncTargetProperty[] _targetPropertyByIndexLookup;

        private readonly Dictionary<string, SyncTargetProperty> _targetPropertyByNameLookup =
            new Dictionary<string, SyncTargetProperty>();

        private bool _constructing;
        private Action<List<object>> _constructor;
        private readonly TypeAccessor _typeAccessor;

        public new INotifyPropertyChanged BaseObject
        {
            get => (INotifyPropertyChanged) base.BaseObject;
            set => base.BaseObject = value;
        }

        public NotifyPropertyChangedSyncTarget(int referenceId, Type baseType, ExtendedBinaryReader reader,
            SyncTargetRoot syncTargetRoot,
            IFieldSerializerResolver fieldDeserializerResolver) : base(referenceId)
        {
            _syncTargetRoot = syncTargetRoot;
            _typeAccessor = TypeAccessor.Create(baseType, false);

            ConstructorInfo attributeMarkedConstructor = GetAttributeMarkedConstructor(baseType);
            var attributeMarkedConstructorPresent = attributeMarkedConstructor != null;

            BaseObject = (INotifyPropertyChanged) (attributeMarkedConstructorPresent
                ? FormatterServices.GetUninitializedObject(baseType)
                : Activator.CreateInstance(baseType));
            BaseObject.PropertyChanged += TargetOnPropertyChanged;

            SyncPropertyInfo[] syncPropertiesInfo = GetProperties();

            _targetPropertyByIndexLookup = new SyncTargetProperty[syncPropertiesInfo.Length];
            for (var syncPropertyIndex = 0; syncPropertyIndex < syncPropertiesInfo.Length; syncPropertyIndex++)
            {
                var syncPropertyInfo = syncPropertiesInfo[syncPropertyIndex];
                SyncTargetProperty syncTargetProperty = CreateSyncTargetProperty(fieldDeserializerResolver, syncPropertyInfo.PropertyInfo);
                _targetPropertyByNameLookup[syncPropertyInfo.PropertyInfo.Name] = syncTargetProperty;
                _targetPropertyByIndexLookup[syncPropertyIndex] = syncTargetProperty;
            }

            Read(reader);

            _constructor = constructionPath =>
            {
                if (attributeMarkedConstructorPresent)
                {
                    InvokeAttributeMarkedConstructor(attributeMarkedConstructor, syncPropertiesInfo, constructionPath);
                }
                else
                {
                    InitializeProperties(syncPropertiesInfo);
                }

                foreach (MethodInfo synchronizedMarkedMethod in GetSynchronizedMarkedMethods(baseType))
                {
                    if (synchronizedMarkedMethod.GetParameters().Length != 0)
                    {
                        throw new ParameterizedSynchronizedCallbackException();
                    }

                    synchronizedMarkedMethod.Invoke(BaseObject, new object[0]);
                }
            };
            _syncTargetRoot.EndRead += SyncTargetRootOnEndRead;
        }

        private SyncTargetProperty CreateSyncTargetProperty(IFieldSerializerResolver fieldDeserializerResolver, PropertyInfo propertyInfo)
        {
            IFieldSerializer fieldSerializer = fieldDeserializerResolver.ResolveSerializer(propertyInfo.PropertyType);

            Action<object> setter = propertyInfo.SetMethod != null
                ? (Action<object>) (value => { _typeAccessor[BaseObject, propertyInfo.Name] = value; })
                : null;

            Func<object> getter = propertyInfo.GetMethod != null
                ? (Func<object>) (() => _typeAccessor[BaseObject, propertyInfo.Name])
                : null;

            var syncTargetProperty = new SyncTargetProperty(
                propertyInfo,
                setter,
                getter,
                _syncTargetRoot, fieldSerializer);
            return syncTargetProperty;
        }

        private void InitializeProperties(SyncPropertyInfo[] syncProperties)
        {
            for (var index = 0;
                index < _targetPropertyByIndexLookup.Length;
                index++)
            {
                SyncTargetProperty syncTargetProperty = _targetPropertyByIndexLookup[index];

                SynchronizationBehaviour attributeSynchronizationBehaviour = syncProperties[index].SyncAttribute.SynchronizationBehaviour;
                if (attributeSynchronizationBehaviour != SynchronizationBehaviour.Ignore)
                {
                    syncTargetProperty.Property = syncTargetProperty.SynchronizedValue;
                }

                syncTargetProperty.SynchronizationBehaviour = attributeSynchronizationBehaviour;
            }
        }

        /// <summary>
        /// Gets the Property name of the constructor parameter.
        /// </summary>
        private static string GetConstructorParameterPropertyName(
            ParameterInfo parameterInfo)
        {
            SyncConstructorParameterAttribute syncConstructorParameterAttribute = parameterInfo.GetCustomAttributes()
                .OfType<SyncConstructorParameterAttribute>()
                .FirstOrDefault();
            var name = syncConstructorParameterAttribute?.PropertyName;

            // If Property doesn't have a SyncConstructorParameter attribute, use PascalCase.
            return name ?? CapitalizeFirstLetter(parameterInfo.Name);
        }

        private void InvokeAttributeMarkedConstructor(ConstructorInfo constructor,
            SyncPropertyInfo[] syncProperties, List<object> constructionPath)
        {
            ParameterInfo[] constructorParametersInfos = constructor.GetParameters();
            
            object[] constructorParameters = new object[constructorParametersInfos.Length];
            var syncTargetPropertyParameters = new HashSet<SyncTargetProperty>();

            for (var i = 0; i < constructorParametersInfos.Length; i++)
            {
                ParameterInfo constructorParametersInfo = constructorParametersInfos[i];

                object ResolveParameterValue()
                {
                    foreach (Attribute customAttribute in constructorParametersInfo.GetCustomAttributes())
                    {
                        // Resolve property parameter explicit with attribute
                        if (customAttribute is SyncConstructorParameterAttribute constructorParameterAttribute)
                        {
                            if (_targetPropertyByNameLookup.TryGetValue(constructorParameterAttribute.PropertyName, out SyncTargetProperty explicitSyncTargetProperty))
                            {
                                syncTargetPropertyParameters.Add(explicitSyncTargetProperty);
                                return explicitSyncTargetProperty.SynchronizedValue;
                            }
                            throw new SyncTargetPropertyNotFoundException(constructorParameterAttribute.PropertyName);
                        }
                        // Resolve dependency parameter explicit with attribute
                        if (customAttribute is SyncDependencyAttribute)
                        {
                            return _syncTargetRoot.Settings.DependencyResolver.ResolveDependency(constructorParametersInfo.ParameterType);
                        }
                    }
                    // Resolve property implicit
                    // If Property doesn't have a SyncConstructorParameter attribute, use PascalCase.
                    string propertyName = CapitalizeFirstLetter(constructorParametersInfo.Name);
                    if (_targetPropertyByNameLookup.TryGetValue(propertyName, out SyncTargetProperty implicitSyncTargetProperty))
                    {
                        syncTargetPropertyParameters.Add(implicitSyncTargetProperty);
                        return implicitSyncTargetProperty.SynchronizedValue;
                    }
                    //  Resolve dependency implicid
                    return _syncTargetRoot.Settings.DependencyResolver.ResolveDependency(constructorParametersInfo.ParameterType);
                }

                constructorParameters[i] = ResolveParameterValue();
            }

            // Make sure properties are initialized before invoking constructor
            foreach (SyncTargetProperty syncTargetProperty in syncTargetPropertyParameters)
            {
                ConstructProperty(syncTargetProperty, constructionPath);
            }

            constructor.Invoke(BaseObject, constructorParameters);

            // Initialize properties that where not given as constructor parameters
            for (var index = 0; index < _targetPropertyByIndexLookup.Length; index++)
            {
                SyncTargetProperty syncTargetProperty = _targetPropertyByIndexLookup[index];

                syncTargetProperty.SynchronizationBehaviour = syncProperties[index].SyncAttribute.SynchronizationBehaviour;
               
                // Skip properties that are initialized from constructor
                if (syncTargetPropertyParameters.Contains(syncTargetProperty))
                {
                    continue;
                }

                syncTargetProperty.Property = syncTargetProperty.SynchronizedValue;
            }
        }

        private void ConstructProperty(SyncTargetProperty syncTargetProperty, List<object> constructionPath)
        {
            object synchronizedValue = syncTargetProperty.SynchronizedValue;
            if (synchronizedValue != null)
            {
                Type type = synchronizedValue.GetType();
                if (type.IsValueType == false)
                {
                    SyncTarget target = _syncTargetRoot.TargetReferencePool.GetSyncObject(synchronizedValue);
                    if (target is NotifyPropertyChangedSyncTarget notifyPropertyChangedSyncTarget)
                    {
                        notifyPropertyChangedSyncTarget.Construct(constructionPath);
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

        private static string CapitalizeFirstLetter(string input)
        {
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        private SyncPropertyInfo[] GetProperties()
        {
            SyncPropertyInfo[] syncProperties =
                SyncPropertyResolver.GetSyncProperties(BaseObject.GetType()).ToArray();
            return syncProperties;
        }

        private static ConstructorInfo GetAttributeMarkedConstructor(Type baseType)
        {
            return baseType.GetConstructors()
                .FirstOrDefault(constructorInfo =>
                    constructorInfo.GetCustomAttributes()
                        .Any(a => a is SyncConstructorAttribute));
        }

        private static IEnumerable<MethodInfo> GetSynchronizedMarkedMethods(Type baseType)
        {
            return baseType.GetMethods().Where(method =>
                method.GetCustomAttributes()
                    .Any(a => a is OnSynchronizedAttribute)).ToList();
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
            if (!_targetPropertyByNameLookup.TryGetValue(e.PropertyName, out SyncTargetProperty syncTargetProperty))
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