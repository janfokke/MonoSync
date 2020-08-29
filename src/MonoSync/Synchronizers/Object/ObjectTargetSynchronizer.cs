using System;
using System.Collections.Concurrent;
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
        protected readonly SynchronizableTargetMember[] SynchronizableTargetMembers;
        private List<Action<object>> _constructionCallbacks;

        public void InvokeWhenConstructed(Action<object> callback)
        {
            if (_state == State.Constructed)
            {
                callback(Reference);
                return;
            }
            _constructionCallbacks ??=new List<Action<object>>();
            _constructionCallbacks.Add(callback);
        }

        private State _state;

        enum State : byte
        {
            Uninitialized,
            Constructing,
            Constructed
        }

        public ObjectTargetSynchronizer(TargetSynchronizerRoot targetSynchronizerRoot, int referenceId, Type referenceType) : base(referenceId)
        {
            _targetSynchronizerRoot = targetSynchronizerRoot;
            Reference = FormatterServices.GetUninitializedObject(referenceType);

            SynchronizableMember[] synchronizableMembers = targetSynchronizerRoot.SynchronizableMemberFactory.FromType(referenceType);

            SynchronizableTargetMembers = new SynchronizableTargetMember[synchronizableMembers.Length];
            for (var syncPropertyIndex = 0; syncPropertyIndex < synchronizableMembers.Length; syncPropertyIndex++)
            {
                SynchronizableMember synchronizableMember = synchronizableMembers[syncPropertyIndex];
                var synchronizableTargetMember = new SynchronizableTargetMember(Reference, synchronizableMember, _targetSynchronizerRoot);
                SynchronizableTargetMembers[syncPropertyIndex] = synchronizableTargetMember;
            }
            _targetSynchronizerRoot.EndRead += TargetSynchronizerRootOnEndRead;
        }

        public bool TryGetMemberByName(string memberName, out SynchronizableTargetMember synchronizableTargetMember)
        {
            for (var i = 0; i < SynchronizableTargetMembers.Length; i++)
            {
                SynchronizableTargetMember x = SynchronizableTargetMembers[i];
                if (x.Name == memberName)
                {
                    synchronizableTargetMember = x;
                    return true;
                }
            }
            synchronizableTargetMember = default;
            return false;
        }

        private void ConstructMemberConstructorParameters(SynchronizableTargetMember synchronizableTargetMember, List<object> constructionPath)
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

        private string ToGenericMemberName(string memberName)
        {
            memberName = memberName.TrimStart('_');
            if (memberName != string.Empty && char.IsUpper(memberName[0]))
            {
                memberName = char.ToLower(memberName[0]) + memberName.Substring(1);
            }
            return memberName;
        }

        private void Construct(List<object> path)
        {
            path.Add(Reference);

            if (_state == State.Uninitialized)
            {
                _state = State.Constructing;
                ConstructorInfo constructor = ResolveConstructor(Reference.GetType());
                ParameterInfo[] constructorParametersInfos = constructor.GetParameters();
                object[] constructorParameters;
                if (constructorParametersInfos.Length > 0)
                {
                    constructorParameters = new object[constructorParametersInfos.Length];
                    var synchronizableConstructorParameters = new HashSet<SynchronizableTargetMember>();

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
                                    if (TryGetMemberByName(constructorParameterAttribute.PropertyName, out SynchronizableTargetMember explicitSyncTargetProperty))
                                    {
                                        synchronizableConstructorParameters.Add(explicitSyncTargetProperty);
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
                            var genericMemberName = ToGenericMemberName(constructorParametersInfo.Name);

                            SynchronizableTargetMember implicitSyncTargetProperty = SynchronizableTargetMembers
                                .FirstOrDefault(targetMember => genericMemberName == ToGenericMemberName(targetMember.Name));

                            if (implicitSyncTargetProperty != null)
                            {
                                synchronizableConstructorParameters.Add(implicitSyncTargetProperty);
                                return implicitSyncTargetProperty.SynchronizedValue;
                            }
                            //  Resolve dependency implicit
                            return _targetSynchronizerRoot.ServiceProvider.GetService(constructorParametersInfo.ParameterType) ?? throw new ArgumentNullException($"{constructor.Name}:{constructorParametersInfo.Name}");
                        }
                        constructorParameters[i] = ResolveParameterValue();
                    }

                    // Make sure members are initialized before invoking constructor
                    foreach (SynchronizableTargetMember syncTargetProperty in synchronizableConstructorParameters)
                    {
                        ConstructMemberConstructorParameters(syncTargetProperty, path);
                    }
                }
                else
                {
                    constructorParameters = null;
                }
                constructor.Invoke(Reference, constructorParameters);

                for (var index = 0; index < SynchronizableTargetMembers.Length; index++)
                {
                    SynchronizableTargetMember synchronizableTargetMember = SynchronizableTargetMembers[index];
                    synchronizableTargetMember.SynchronizationBehaviour = synchronizableTargetMember.DefaultSynchronizationBehaviour;

                    if (synchronizableTargetMember.SynchronizationBehaviour != SynchronizationBehaviour.Manual &&
                        synchronizableTargetMember.SynchronizationBehaviour != SynchronizationBehaviour.Construction)
                        synchronizableTargetMember.Value = synchronizableTargetMember.SynchronizedValue;
                }
                
                _state = State.Constructed;

                if (_constructionCallbacks != null)
                {
                    foreach (Action<object> constructionCallback in _constructionCallbacks)
                    {
                        constructionCallback.Invoke(Reference);
                    }
                    _constructionCallbacks = null;
                }
            }
            else if(_state == State.Constructing)
            {
                throw new ConstructorReferenceCycleException(path);
            }
        }

        private static readonly ConcurrentDictionary<Type, ConstructorInfo> ConstructorCache = new ConcurrentDictionary<Type, ConstructorInfo>();
        private static ConstructorInfo ResolveConstructor(Type baseType)
        {
            return ConstructorCache.GetOrAdd(baseType, key =>
            {
                ConstructorInfo[] constructors = key.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance);
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
                    throw new MultipleConstructorsException(key);
                }
                // Default constructor
                return constructors.First();
            });
        }

        private void TargetSynchronizerRootOnEndRead(object sender, EventArgs e)
        {
            Construct(new List<object>());
            _targetSynchronizerRoot.EndRead -= TargetSynchronizerRootOnEndRead;
        }

        public override void Dispose()
        {
            foreach (SynchronizableTargetMember syncTargetProperty in SynchronizableTargetMembers)
            {
                syncTargetProperty.Dispose();
            }
        }

        public override void Read(ExtendedBinaryReader reader)
        {
            for (var index = 0; index < SynchronizableTargetMembers.Length; index++)
            {
                SynchronizableTargetMember synchronizableTargetMember = SynchronizableTargetMembers[index];
                synchronizableTargetMember.ReadChanges(reader);
            }
        }
    }
}