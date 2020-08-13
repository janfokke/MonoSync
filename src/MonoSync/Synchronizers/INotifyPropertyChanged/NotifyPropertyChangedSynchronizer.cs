﻿using System;
using System.ComponentModel;
using System.Linq;
using MonoSync.Attributes;

namespace MonoSync.Synchronizers
{
    internal class NotifyPropertyChangedSynchronizer : ISynchronizer
    {
        public bool CanSynchronize(Type type)
        {
            var any = type.GetCustomAttributes(true).OfType<SynchronizableAttribute>().Any();
            return typeof(INotifyPropertyChanged).IsAssignableFrom(type) && 
                   any; ;
        }

        public SourceSynchronizer Synchronize(SourceSynchronizerRoot sourceSynchronizerRoot, int referenceId, object reference)
        {
            return new NotifyPropertyChangedSourceSynchronizer(sourceSynchronizerRoot, referenceId, (INotifyPropertyChanged) reference);
        }

        public TargetSynchronizer Synchronize(TargetSynchronizerRoot targetSynchronizerRoot, int referenceId, Type referenceType)
        {
            return new NotifyPropertyChangedTargetSynchronizer(targetSynchronizerRoot, referenceId, referenceType);
        }
    }
}