namespace AutoMapperFragmentConfig;

[AttributeUsage(AttributeTargets.Method)]
public sealed class MapExtensionAttribute : Attribute
{
    public string ProfileName { get; }

    public MapExtensionAttribute()
        : this(string.Empty)
    {
    }

    public MapExtensionAttribute(string profileName)
    {
        ProfileName = profileName;
    }
}
