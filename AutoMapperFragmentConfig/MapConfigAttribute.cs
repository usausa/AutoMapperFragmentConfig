namespace AutoMapperFragmentConfig;

[AttributeUsage(AttributeTargets.Method)]
public sealed class MapConfigAttribute : Attribute
{
    public string ProfileName { get; }

    public MapConfigAttribute()
        : this(string.Empty)
    {
    }

    public MapConfigAttribute(string profileName)
    {
        ProfileName = profileName;
    }
}
