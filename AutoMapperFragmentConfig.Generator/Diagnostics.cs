namespace AutoMapperFragmentConfig.Generator;

using Microsoft.CodeAnalysis;

internal static class Diagnostics
{
    public static DiagnosticDescriptor InvalidExtensionMethodDefinition { get; } = new(
        id: "AMFC0001",
        title: "Invalid extension method definition",
        messageFormat: "Extension method must be static void. method=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidExtensionMethodParameter { get; } = new(
        id: "AMFC0002",
        title: "Invalid extension method parameter",
        messageFormat: "Parameter type must be IMapperConfigurationExpression and IServiceProvider(option). method=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidConfigMethodDefinition { get; } = new(
        id: "AMFC0003",
        title: "Invalid config method definition",
        messageFormat: "Config method must be static void. method=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidConfigMethodParameter { get; } = new(
        id: "AMFC0004",
        title: "Invalid config method parameter",
        messageFormat: "Parameter type must be IProfileExpression and IServiceProvider(option). method=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor ProviderParameterRequired { get; } = new(
        id: "AMFC0005",
        title: "IServiceProvider parameter required",
        messageFormat: "IServiceProvider parameter required for extension. method=[{0}]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
