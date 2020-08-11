using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using MonoSync.Attributes;
using MonoSync.Exceptions;
using MonoSync.Utils;

namespace MonoSync.Synchronizers
{


    public class ObjectTargetSynchronizer : TargetSynchronizer
    {
        private readonly TargetSynchronizerRoot _targetSynchronizerRoot;
        protected readonly SyncPropertyAccessor[] TargetPropertyByIndexLookup;
        protected readonly Dictionary<string, SyncPropertyAccessor> TargetPropertyByNameLookup = new Dictionary<string, SyncPropertyAccessor>();
        
        private bool _constructing;
        private Action<List<object>> _constructor;

        public ObjectTargetSynchronizer(TargetSynchronizerRoot targetSynchronizerRoot, int referenceId, Type referenceType) : base(referenceId)
        {
            _targetSynchronizerRoot = targetSynchronizerRoot;
            Reference = FormatterServices.GetUninitializedObject(referenceType);
            SynchronizablePropertyInfo[] syncPropertiesInfo = GetProperties();

            TargetPropertyByIndexLookup = new SyncPropertyAccessor[syncPropertiesInfo.Length];
            for (var syncPropertyIndex = 0; syncPropertyIndex < syncPropertiesInfo.Length; syncPropertyIndex++)
            {
                var syncPropertyInfo = syncPropertiesInfo[syncPropertyIndex];
                SyncPropertyAccessor syncPropertyAccessor = CreateSyncTargetProperty(targetSynchronizerRoot.Settings.Serializers, syncPropertyInfo.PropertyInfo);
                TargetPropertyByNameLookup[syncPropertyInfo.PropertyInfo.Name] = syncPropertyAccessor;
                TargetPropertyByIndexLookup[syncPropertyIndex] = syncPropertyAccessor;
            }

            _constructor = constructionPath =>
            {
                ConstructorInfo constructor = ResolveConstructor(referenceType);
                InvokeConstructor(constructor, syncPropertiesInfo, constructionPath);
            };
            _targetSynchronizerRoot.EndRead += TargetSynchronizerRootOnEndRead;
        }

        private SyncPropertyAccessor CreateSyncTargetProperty(SerializerCollection fieldDeserializerResolver, PropertyInfo propertyInfo)
        {
            ISerializer serializer = fieldDeserializerResolver.FindSerializerByType(propertyInfo.PropertyType);

            Action <object> directSetter = null;
            if (ReflectionUtils.TryResolvePropertySetter(out Action<object, object> referenceSetter, propertyInfo))
            {
                directSetter = value => referenceSetter(Reference, value);
            }

            Func<object> directGetter = null;
            if (ReflectionUtils.TryResolvePropertyGetter(out Func<object, object> referenceGetter, propertyInfo))
            {
                directGetter = () => referenceGetter(Reference);
            }

            var syncTargetProperty = new SyncPropertyAccessor(
                propertyInfo,
                directSetter,
                directGetter,
                _targetSynchronizerRoot, serializer);
            return syncTargetProperty;
        }

        private void InvokeConstructor(ConstructorInfo constructor,
            SynchronizablePropertyInfo[] syncProperties, List<object> constructionPath)
        {
            ParameterInfo[] constructorParametersInfos = constructor.GetParameters();
            
            object[] constructorParameters = new object[constructorParametersInfos.Length];
            var syncTargetPropertyParameters = new HashSet<SyncPropertyAccessor>();

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
                            if (TargetPropertyByNameLookup.TryGetValue(constructorParameterAttribute.PropertyName, out SyncPropertyAccessor explicitSyncTargetProperty))
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
                    if (TargetPropertyByNameLookup.TryGetValue(propertyName, out SyncPropertyAccessor implicitSyncTargetProperty))
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
            foreach (SyncPropertyAccessor syncTargetProperty in syncTargetPropertyParameters)
            {
                ConstructProperty(syncTargetProperty, constructionPath);
            }

            constructor.Invoke(Reference, constructorParameters);

            for (var index = 0; index < TargetPropertyByIndexLookup.Length; index++)
            {
                SyncPropertyAccessor syncPropertyAccessor = TargetPropertyByIndexLookup[index];

                syncPropertyAccessor.SynchronizationBehaviour = syncProperties[index].SynchronizeAttribute.SynchronizationBehaviour;
                
                if(syncPropertyAccessor.SynchronizationBehaviour != SynchronizationBehaviour.Manual &&
                   syncPropertyAccessor.SynchronizationBehaviour != SynchronizationBehaviour.Construction)
                    syncPropertyAccessor.Property = syncPropertyAccessor.SynchronizedValue;
            }
        }

        private void ConstructProperty(SyncPropertyAccessor syncPropertyAccessor, List<object> constructionPath)
        {
            object synchronizedValue = syncPropertyAccessor.SynchronizedValue;
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

        private SynchronizablePropertyInfo[] GetProperties()
        {
            SynchronizablePropertyInfo[] syncProperties =
                SynchronizablePropertyInfo.FromType(Reference.GetType()).ToArray();
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

        internal SyncPropertyAccessor GetSyncTargetProperty(string propertyName)
        {
            if (TargetPropertyByNameLookup.TryGetValue(propertyName, out SyncPropertyAccessor property))
            {
                return property;
            }

            throw new SyncTargetPropertyNotFoundException(propertyName);
        }

        public override void Dispose()
        {
            foreach (SyncPropertyAccessor syncTargetProperty in TargetPropertyByIndexLookup)
            {
                syncTargetProperty.Dispose();
            }
        }

        public override void Read(ExtendedBinaryReader reader)
        {
            for (var index = 0; index < TargetPropertyByIndexLookup.Length; index++)
            {
                SyncPropertyAccessor syncPropertyAccessor = TargetPropertyByIndexLookup[index];
                syncPropertyAccessor.ReadChanges(reader);
            }
        }
    }
}