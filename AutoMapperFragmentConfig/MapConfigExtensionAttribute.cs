namespace AutoMapperFragmentConfig;

[AttributeUsage(AttributeTargets.Method)]
public sealed class MapConfigExtensionAttribute : Attribute
{
    public string ProfileName { get; }

    public MapConfigExtensionAttribute()
        : this(string.Empty)
    {
    }

    public MapConfigExtensionAttribute(string profileName)
    {
        ProfileName = profileName;
    }
}
