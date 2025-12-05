namespace AutoMapperFragmentConfig.Generator;

using Microsoft.CodeAnalysis;

internal static class Diagnostics
{
    public static DiagnosticDescriptor InvalidExtensionMethodDefinition => new(
        id: "AMFC0001",
        title: "Invalid extension method definition",
        messageFormat: "Extension method must be static void. method=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidExtensionMethodParameter => new(
        id: "AMFC0002",
        title: "Invalid extension method parameter",
        messageFormat: "Parameter type must be IMapperConfigurationExpression and IServiceProvider(option). method=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidConfigMethodDefinition => new(
        id: "AMFC0003",
        title: "Invalid config method definition",
        messageFormat: "Config method must be static void. method=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidConfigMethodParameter => new(
        id: "AMFC0004",
        title: "Invalid config method parameter",
        messageFormat: "Parameter type must be IProfileExpression and IServiceProvider(option). method=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor ProviderParameterRequired => new(
        id: "AMFC0005",
        title: "IServiceProvider parameter required",
        messageFormat: "IServiceProvider parameter required for extension. method=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
