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
        protected readonly SynchronizableTargetMember[] TargetMemberByIndexLookup;
        protected readonly Dictionary<string, SynchronizableTargetMember> TargetMemberByNameLookup = new Dictionary<string, SynchronizableTargetMember>();
        
        private bool _constructing;
        private Action<List<object>> _constructor;

        public ObjectTargetSynchronizer(TargetSynchronizerRoot targetSynchronizerRoot, int referenceId, Type referenceType) : base(referenceId)
        {
            _targetSynchronizerRoot = targetSynchronizerRoot;
            Reference = FormatterServices.GetUninitializedObject(referenceType);

            SynchronizableMember[] synchronizableMembers = targetSynchronizerRoot.SynchronizableMemberFactory.FromType(referenceType);

            TargetMemberByIndexLookup = new SynchronizableTargetMember[synchronizableMembers.Length];
            for (var syncPropertyIndex = 0; syncPropertyIndex < synchronizableMembers.Length; syncPropertyIndex++)
            {
                SynchronizableMember synchronizableMember = synchronizableMembers[syncPropertyIndex];
                var synchronizableTargetMember = new SynchronizableTargetMember(Reference, synchronizableMember, _targetSynchronizerRoot);
                TargetMemberByNameLookup[synchronizableMember.MemberInfo.Name] = synchronizableTargetMember;
                TargetMemberByIndexLookup[syncPropertyIndex] = synchronizableTargetMember;
            }

            _constructor = constructionPath =>
            {
                ConstructorInfo constructor = ResolveConstructor(referenceType);
                InvokeConstructor(constructor, synchronizableMembers, constructionPath);
            };
            _targetSynchronizerRoot.EndRead += TargetSynchronizerRootOnEndRead;
        }

        private void InvokeConstructor(ConstructorInfo constructor,
            SynchronizableMember[] synchronizableMembers, List<object> constructionPath)
        {
            ParameterInfo[] constructorParametersInfos = constructor.GetParameters();
            
            object[] constructorParameters = new object[constructorParametersInfos.Length];
            var syncTargetPropertyParameters = new HashSet<SynchronizableTargetMember>();

            for (var i = 0; i < constructorParametersInfos.Length; i++)
            {
                ParameterInfo constructorParametersInfo = constructorParametersInfos[i];

                object ResolveParameterValue()
                {
                    foreach (Attribute customAttribute in constructorParametersInfo.GetCustomAttributes())
                    {
                        // Resolve property parameter explicit with attribute
                        if (customAttribute is SynchronizationParameterAttribute constructorParameterAttribute)
                        {
                            if (TargetMemberByNameLookup.TryGetValue(constructorParameterAttribute.PropertyName, out SynchronizableTargetMember explicitSyncTargetProperty))
                            {
                                syncTargetPropertyParameters.Add(explicitSyncTargetProperty);
                                return explicitSyncTargetProperty.SynchronizedValue;
                            }
                            throw new SyncTargetMemberNotFoundException(constructorParameterAttribute.PropertyName);
                        }
                        // Resolve dependency parameter explicit with attribute
                        if (customAttribute is SynchronizationDependencyAttribute)
                        {
                            return _targetSynchronizerRoot.ServiceProvider.GetService(constructorParametersInfo.ParameterType) ?? throw new ArgumentNullException($"{constructor.Name}:{constructorParametersInfo.Name}");
                        }
                    }
                    // Resolve property implicit
                    // If Value doesn't have a SyncConstructorParameter attribute, use PascalCase.
                    var  genericMemberName = ToGenericMemberName(constructorParametersInfo.Name);

                    SynchronizableTargetMember implicitSyncTargetProperty = TargetMemberByNameLookup
                        .FirstOrDefault(keyValuePair => genericMemberName == ToGenericMemberName(keyValuePair.Key))
                        .Value;

                    if (implicitSyncTargetProperty != null)
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
            foreach (SynchronizableTargetMember syncTargetProperty in syncTargetPropertyParameters)
            {
                ConstructProperty(syncTargetProperty, constructionPath);
            }

            constructor.Invoke(Reference, constructorParameters);

            for (var index = 0; index < TargetMemberByIndexLookup.Length; index++)
            {
                SynchronizableTargetMember synchronizableTargetMember = TargetMemberByIndexLookup[index];
                synchronizableTargetMember.SynchronizationBehaviour = synchronizableMembers[index].SynchronizeAttribute.SynchronizationBehaviour;
                
                if(synchronizableTargetMember.SynchronizationBehaviour != SynchronizationBehaviour.Manual &&
                   synchronizableTargetMember.SynchronizationBehaviour != SynchronizationBehaviour.Construction)
                    synchronizableTargetMember.Value = synchronizableTargetMember.SynchronizedValue;
            }
        }

        private string ToGenericMemberName(string memberName)
        {
            memberName = memberName.TrimStart('_');
            if (memberName != string.Empty && char.IsUpper(memberName[0]))
            {
                memberName = char.ToLower(memberName[0]) + memberName.Substring(1);
            }
            return memberName;
        }

        private void ConstructProperty(SynchronizableTargetMember synchronizableTargetMember, List<object> constructionPath)
        {
            object synchronizedValue = synchronizableTargetMember.SynchronizedValue;
            if (synchronizedValue != null)
            {
                Type type = synchronizedValue.GetType();
                if (type.IsValueType == false)
                {
                    TargetSynchronizer target = _targetSynchronizerRoot.ReferencePool.GetSynchronizer(synchronizedValue);
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

        private static ConstructorInfo ResolveConstructor(Type baseType)
        {
            ConstructorInfo[] constructors = baseType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance);

            // Prioritize constructor with SynchronizationConstructorAttribute
            foreach (ConstructorInfo constructorInfo in constructors)
            {
                if (constructorInfo.GetCustomAttributes().Any(a => a is SynchronizationConstructorAttribute))
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

        internal SynchronizableTargetMember GetSyncTargetMember(string memberName)
        {
            if (TargetMemberByNameLookup.TryGetValue(memberName, out SynchronizableTargetMember property))
            {
                return property;
            }
            throw new SyncTargetMemberNotFoundException(memberName);
        }

        public override void Dispose()
        {
            foreach (SynchronizableTargetMember syncTargetProperty in TargetMemberByIndexLookup)
            {
                syncTargetProperty.Dispose();
            }
        }

        public override void Read(ExtendedBinaryReader reader)
        {
            for (var index = 0; index < TargetMemberByIndexLookup.Length; index++)
            {
                SynchronizableTargetMember synchronizableTargetMember = TargetMemberByIndexLookup[index];
                synchronizableTargetMember.ReadChanges(reader);
            }
        }
    }
}