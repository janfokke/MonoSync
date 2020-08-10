using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using FastMember;
using MonoSync.Attributes;
using MonoSync.Exceptions;
using MonoSync.Utils;

namespace MonoSync.Synchronizers
{
    public class ObjectTargetSynchronizer : TargetSynchronizer
    {
        private readonly TargetSynchronizerRoot _targetSynchronizerRoot;
        protected readonly SyncTargetProperty[] TargetPropertyByIndexLookup;
        protected readonly Dictionary<string, SyncTargetProperty> TargetPropertyByNameLookup = new Dictionary<string, SyncTargetProperty>();
        protected readonly TypeAccessor TypeAccessor;

        private bool _constructing;
        private Action<List<object>> _constructor;

        public ObjectTargetSynchronizer(TargetSynchronizerRoot targetSynchronizerRoot, int referenceId, Type referenceType) : base(referenceId)
        {
            _targetSynchronizerRoot = targetSynchronizerRoot;
            TypeAccessor = TypeAccessor.Create(referenceType, false);
            Reference = FormatterServices.GetUninitializedObject(referenceType);
            SyncPropertyInfo[] syncPropertiesInfo = GetProperties();

            TargetPropertyByIndexLookup = new SyncTargetProperty[syncPropertiesInfo.Length];
            for (var syncPropertyIndex = 0; syncPropertyIndex < syncPropertiesInfo.Length; syncPropertyIndex++)
            {
                var syncPropertyInfo = syncPropertiesInfo[syncPropertyIndex];
                SyncTargetProperty syncTargetProperty = CreateSyncTargetProperty(targetSynchronizerRoot.Settings.Serializers, syncPropertyInfo.PropertyInfo);
                TargetPropertyByNameLookup[syncPropertyInfo.PropertyInfo.Name] = syncTargetProperty;
                TargetPropertyByIndexLookup[syncPropertyIndex] = syncTargetProperty;
            }

            _constructor = constructionPath =>
            {
                ConstructorInfo constructor = ResolveConstructor(referenceType);
                InvokeConstructor(constructor, syncPropertiesInfo, constructionPath);
            };
            _targetSynchronizerRoot.EndRead += TargetSynchronizerRootOnEndRead;
        }

        private SyncTargetProperty CreateSyncTargetProperty(SerializerCollection fieldDeserializerResolver, PropertyInfo propertyInfo)
        {
            ISerializer serializer = fieldDeserializerResolver.FindSerializerByType(propertyInfo.PropertyType);

            Action<object> setter = propertyInfo.SetMethod != null
                ? (Action<object>) (value => { TypeAccessor[Reference, propertyInfo.Name] = value; })
                : null;

            Func<object> getter = propertyInfo.GetMethod != null
                ? (Func<object>) (() => TypeAccessor[Reference, propertyInfo.Name])
                : null;

            var syncTargetProperty = new SyncTargetProperty(
                propertyInfo,
                setter,
                getter,
                _targetSynchronizerRoot, serializer);
            return syncTargetProperty;
        }

        private void InvokeConstructor(ConstructorInfo constructor,
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
                            if (TargetPropertyByNameLookup.TryGetValue(constructorParameterAttribute.PropertyName, out SyncTargetProperty explicitSyncTargetProperty))
                            {
                                syncTargetPropertyParameters.Add(explicitSyncTargetProperty);
                                return explicitSyncTargetProperty.SynchronizedValue;
                            }
                            throw new SyncTargetPropertyNotFoundException(constructorParameterAttribute.PropertyName);
                        }
                        // Resolve dependency parameter explicit with attribute
                        if (customAttribute is SyncDependencyAttribute)
                        {
                            return _targetSynchronizerRoot.ServiceProvider.GetService(constructorParametersInfo.ParameterType) ?? throw new ArgumentNullException($"{constructor.Name}:{constructorParametersInfo.Name}");
                        }
                    }
                    // Resolve property implicit
                    // If Property doesn't have a SyncConstructorParameter attribute, use PascalCase.
                    string propertyName = CapitalizeFirstLetter(constructorParametersInfo.Name);
                    if (TargetPropertyByNameLookup.TryGetValue(propertyName, out SyncTargetProperty implicitSyncTargetProperty))
                    {
                        syncTargetPropertyParameters.Add(implicitSyncTargetProperty);
                        return implicitSyncTargetProperty.SynchronizedValue;
                    }
                    //  Resolve dependency implicit
                    return _targetSynchronizerRoot.ServiceProvider.GetService(constructorParametersInfo.ParameterType) ?? throw new ArgumentNullException($"{constructor.Name}:{constructorParametersInfo.Name}");
                }

                constructorParameters[i] = ResolveParameterValue();
            }

            // Make sure properties are initialized before invoking constructor
            foreach (SyncTargetProperty syncTargetProperty in syncTargetPropertyParameters)
            {
                ConstructProperty(syncTargetProperty, constructionPath);
            }

            constructor.Invoke(Reference, constructorParameters);

            for (var index = 0; index < TargetPropertyByIndexLookup.Length; index++)
            {
                SyncTargetProperty syncTargetProperty = TargetPropertyByIndexLookup[index];

                syncTargetProperty.SynchronizationBehaviour = syncProperties[index].SynchronizeAttribute.SynchronizationBehaviour;
                
                if(syncTargetProperty.SynchronizationBehaviour != SynchronizationBehaviour.Manual &&
                   syncTargetProperty.SynchronizationBehaviour != SynchronizationBehaviour.Construction)
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
                    TargetSynchronizer target = _targetSynchronizerRoot.ReferencePool.GetSyncObject(synchronizedValue);
                    if (target is ObjectTargetSynchronizer notifyPropertyChangedSyncTarget)
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

            path.Add(Reference);

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
                SyncPropertyResolver.GetSyncProperties(Reference.GetType()).ToArray();
            return syncProperties;
        }

        private static ConstructorInfo ResolveConstructor(Type baseType)
        {
            ConstructorInfo[] constructors = baseType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance);

            // Prioritize constructor with SyncConstructorAttribute
            foreach (ConstructorInfo constructorInfo in constructors)
            {
                if (constructorInfo.GetCustomAttributes().Any(a => a is SyncConstructorAttribute))
                {
                    return constructorInfo;
                }
            }

            if (constructors.Length > 1)
            {
                throw new MultipleConstructorsException(baseType);
            }

            // Default constructor
            return constructors.First();
        }

        private void TargetSynchronizerRootOnEndRead(object sender, EventArgs e)
        {
            Construct(new List<object>());
            _targetSynchronizerRoot.EndRead -= TargetSynchronizerRootOnEndRead;
        }

        internal SyncTargetProperty GetSyncTargetProperty(string propertyName)
        {
            if (TargetPropertyByNameLookup.TryGetValue(propertyName, out SyncTargetProperty property))
            {
                return property;
            }

            throw new SyncTargetPropertyNotFoundException(propertyName);
        }

        public override void Dispose()
        {
            foreach (SyncTargetProperty syncTargetProperty in TargetPropertyByIndexLookup)
            {
                syncTargetProperty.Dispose();
            }
        }

        public override void Read(ExtendedBinaryReader reader)
        {
            for (var index = 0; index < TargetPropertyByIndexLookup.Length; index++)
            {
                SyncTargetProperty syncTargetProperty = TargetPropertyByIndexLookup[index];
                syncTargetProperty.ReadChanges(reader);
            }
        }
    }
}