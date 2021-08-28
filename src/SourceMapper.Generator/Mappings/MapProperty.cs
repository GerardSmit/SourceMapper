using Space.SourceGenerator.Client;

namespace SourceMapper.Generator
{
    public class MapProperty
    {
        public MapProperty(PropertyInfo sourceProperty, PropertyInfo targetProperty)
        {
            SourceProperty = sourceProperty;
            TargetProperty = targetProperty;
        }

        public PropertyInfo SourceProperty { get; }

        public PropertyInfo TargetProperty { get; }
    }
}