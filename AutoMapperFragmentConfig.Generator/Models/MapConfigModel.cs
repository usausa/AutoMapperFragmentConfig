namespace AutoMapperFragmentConfig.Generator.Models;

internal sealed record MapConfigModel(
    string FullClassName,
    string MethodName,
    bool HasProviderParameter,
    string ProfileName);
