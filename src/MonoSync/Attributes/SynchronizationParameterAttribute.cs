using System;

namespace MonoSync.Attributes
{
    /// <summary>
    ///     <see cref="SynchronizationParameterAttribute" /> is used when the name cannot be resolved implicitly
    ///     by capitalizing the first letter of the parameter.
    /// </summary>
    /// <remarks><see cref="PropertyName" /> must be a public property with the <see cref="SynchronizeAttribute" /> attribute</remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class SynchronizationParameterAttribute : Attribute
    {
        public string PropertyName { get; }

        public SynchronizationParameterAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}