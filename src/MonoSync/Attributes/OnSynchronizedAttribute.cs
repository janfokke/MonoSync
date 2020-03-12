using System;
using System.ComponentModel;

namespace MonoSync.Attributes
{
    /// <summary>
    ///     Gets called after the construction of a <see cref="INotifyPropertyChanged" /> sync object
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Method)]
    public class OnSynchronizedAttribute : Attribute
    {
    }
}