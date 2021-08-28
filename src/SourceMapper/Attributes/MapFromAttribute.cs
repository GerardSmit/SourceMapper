using System;

namespace SourceMapper
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class MapFromAttribute : Attribute
    {
        public MapFromAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}
