namespace AutoMapperFragmentConfig.Generator.Models;

using Microsoft.CodeAnalysis;

internal sealed record MapConfigExtensionModel(
    string Namespace,
    string ClassName,
    bool IsValueType,
    Accessibility MethodAccessibility,
    string MethodName,
    string ExpressionParameterName,
    string ProviderParameterName,
    string ProfileName);
