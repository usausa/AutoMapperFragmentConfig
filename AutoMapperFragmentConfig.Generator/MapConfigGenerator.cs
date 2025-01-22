namespace AutoMapperFragmentConfig.Generator;

using System;
using System.Collections.Immutable;
using System.Text;

using AutoMapperFragmentConfig.Generator.Helpers;
using AutoMapperFragmentConfig.Generator.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public sealed class MapConfigGenerator : IIncrementalGenerator
{
    private const string MapConfigAttributeName = "AutoMapperFragmentConfig.MapConfigAttribute";
    private const string MapExtensionAttributeName = "AutoMapperFragmentConfig.MapConfigExtensionAttribute";

    private const string AutoMapperMapperConfigurationExpressionName = "AutoMapper.IMapperConfigurationExpression";
    private const string AutoMapperProfileExpressionName = "AutoMapper.IProfileExpression";

    private const string ServiceProviderName = "System.IServiceProvider";

    // ------------------------------------------------------------
    // Initialize
    // ------------------------------------------------------------

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var mapExtensionProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                MapExtensionAttributeName,
                static (node, _) => IsMapExtensionTargetSyntax(node),
                static (context, _) => GetMapExtensionModel(context))
            .Collect();

        var mapConfigProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                MapConfigAttributeName,
                static (node, _) => IsMapConfigTargetSyntax(node),
                static (context, _) => GetMapConfigModel(context))
            .Collect();

        context.RegisterImplementationSourceOutput(
            mapExtensionProvider.Combine(mapConfigProvider),
            static (context, provider) => Execute(context, provider.Left, provider.Right));
    }

    // ------------------------------------------------------------
    // Parser
    // ------------------------------------------------------------

    private static bool IsMapExtensionTargetSyntax(SyntaxNode node) =>
        node is MethodDeclarationSyntax;

    private static Result<MapConfigExtensionModel> GetMapExtensionModel(GeneratorAttributeSyntaxContext context)
    {
        var syntax = (MethodDeclarationSyntax)context.TargetNode;
        if (context.SemanticModel.GetDeclaredSymbol(syntax) is not IMethodSymbol symbol)
        {
            return Results.Error<MapConfigExtensionModel>(null);
        }

        // Validate method style
        if (!symbol.IsStatic || !symbol.IsPartialDefinition || !symbol.IsExtensionMethod || !symbol.ReturnsVoid)
        {
            // TODO
            return Results.Error<MapConfigExtensionModel>(new DiagnosticInfo(null!, syntax.GetLocation()));
        }

        // Validate argument
        if ((symbol.Parameters.Length != 1) && (symbol.Parameters.Length != 2))
        {
            // TODO
            return Results.Error<MapConfigExtensionModel>(new DiagnosticInfo(null!, syntax.GetLocation()));
        }

        if (symbol.Parameters[0].Type.ToDisplayString() != AutoMapperMapperConfigurationExpressionName)
        {
            // TODO
            return Results.Error<MapConfigExtensionModel>(new DiagnosticInfo(null!, syntax.GetLocation()));
        }

        if ((symbol.Parameters.Length > 1) &&
            (symbol.Parameters[1].Type.ToDisplayString() != ServiceProviderName))
        {
            // TODO
            return Results.Error<MapConfigExtensionModel>(new DiagnosticInfo(null!, syntax.GetLocation()));
        }

        var containingType = symbol.ContainingType;
        var ns = String.IsNullOrEmpty(containingType.ContainingNamespace.Name)
            ? string.Empty
            : containingType.ContainingNamespace.ToDisplayString();

        var attribute = symbol.GetAttributes()
            .FirstOrDefault(static x => x.AttributeClass!.ToDisplayString() == MapExtensionAttributeName);
        var profileName = attribute?.ConstructorArguments.Length > 0
            ? attribute.ConstructorArguments[0].Value!.ToString()
            : "_Fragment";

        return Results.Success(new MapConfigExtensionModel(
            ns,
            containingType.Name,
            containingType.IsValueType,
            symbol.DeclaredAccessibility,
            symbol.Name,
            symbol.Parameters[0].Name,
            symbol.Parameters.Length > 0 ? symbol.Parameters[1].Name : string.Empty,
            profileName));
    }

    private static bool IsMapConfigTargetSyntax(SyntaxNode node) =>
        node is MethodDeclarationSyntax;

    private static Result<MapConfigModel> GetMapConfigModel(GeneratorAttributeSyntaxContext context)
    {
        var syntax = (MethodDeclarationSyntax)context.TargetNode;
        if (context.SemanticModel.GetDeclaredSymbol(syntax) is not IMethodSymbol symbol)
        {
            return Results.Error<MapConfigModel>(null);
        }

        // Validate method style
        if (!symbol.IsStatic || !symbol.ReturnsVoid)
        {
            // TODO
            return Results.Error<MapConfigModel>(new DiagnosticInfo(null!, syntax.GetLocation()));
        }

        // Validate argument
        if ((symbol.Parameters.Length != 1) && (symbol.Parameters.Length != 2))
        {
            // TODO
            return Results.Error<MapConfigModel>(new DiagnosticInfo(null!, syntax.GetLocation()));
        }

        if (symbol.Parameters[0].Type.ToDisplayString() != AutoMapperProfileExpressionName)
        {
            // TODO
            return Results.Error<MapConfigModel>(new DiagnosticInfo(null!, syntax.GetLocation()));
        }

        if ((symbol.Parameters.Length > 1) &&
            (symbol.Parameters[1].Type.ToDisplayString() != ServiceProviderName))
        {
            // TODO
            return Results.Error<MapConfigModel>(new DiagnosticInfo(null!, syntax.GetLocation()));
        }

        var attribute = symbol.GetAttributes()
            .FirstOrDefault(static x => x.AttributeClass!.ToDisplayString() == MapExtensionAttributeName);
        var profileName = attribute?.ConstructorArguments.Length > 0
            ? attribute.ConstructorArguments[0].Value!.ToString()
            : "_Fragment";

        return Results.Success(new MapConfigModel(
            symbol.ContainingType.ToDisplayString(),
            symbol.Name,
            symbol.Parameters.Length > 1,
            profileName));
    }

    // ------------------------------------------------------------
    // Generator
    // ------------------------------------------------------------

    private static void Execute(SourceProductionContext context, ImmutableArray<Result<MapConfigExtensionModel>> extensions, ImmutableArray<Result<MapConfigModel>> configs)
    {
        foreach (var info in extensions.SelectPart(static x => x.Error))
        {
            context.ReportDiagnostic(info);
        }
        foreach (var info in configs.SelectPart(static x => x.Error))
        {
            context.ReportDiagnostic(info);
        }

        var builder = new SourceBuilder();
        foreach (var extension in extensions.SelectPart(static x => x.Value))
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            var targetConfigs = configs
                .SelectPart(static x => x.Value)
                .Where(x => x.ProfileName == extension.ProfileName)
                .ToList();
            if (String.IsNullOrEmpty(extension.ProviderParameterName) &&
                targetConfigs.Any(static x => x.HasProviderParameter))
            {
                // TODO UseProviderチェック
                continue;
            }

            builder.Clear();
            BuildSource(builder, extension, targetConfigs);

            var filename = MakeFilename(extension.Namespace, extension.ClassName);
            var source = builder.ToString();
            context.AddSource(filename, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static void BuildSource(SourceBuilder builder, MapConfigExtensionModel extension, IEnumerable<MapConfigModel> configs)
    {
        builder.AutoGenerated();
        builder.EnableNullable();
        builder.NewLine();

        // namespace
        if (!String.IsNullOrEmpty(extension.Namespace))
        {
            builder.Namespace(extension.Namespace);
            builder.NewLine();
        }

        // class
        builder.Indent().Append("partial ").Append(extension.IsValueType ? "struct " : "class ").Append(extension.ClassName).NewLine();
        builder.BeginScope();

        // method
        builder
            .Indent()
            .Append(extension.MethodAccessibility.ToText())
            .Append(" static partial void ")
            .Append(extension.MethodName)
            .Append("(this ")
            .Append(AutoMapperMapperConfigurationExpressionName)
            .Append(' ')
            .Append(extension.ExpressionParameterName);
        if (!String.IsNullOrEmpty(extension.ProviderParameterName))
        {
            builder
                .Append(", ")
                .Append(ServiceProviderName)
                .Append(' ')
                .Append(extension.ProviderParameterName);
        }
        builder
            .Append(')')
            .NewLine();
        builder.BeginScope();

        builder
            .Indent()
            .Append(extension.ExpressionParameterName)
            .Append(".CreateProfile(\"")
            .Append(extension.ProfileName)
            .Append("\", x =>")
            .NewLine();
        builder.Indent().Append('{').NewLine();
        builder.IndentLevel++;

        foreach (var config in configs)
        {
            builder
                .Indent()
                .Append(config.FullClassName)
                .Append('.')
                .Append(config.MethodName)
                .Append('(')
                .Append(extension.ExpressionParameterName);
            if (config.HasProviderParameter)
            {
                builder
                    .Append(", ")
                    .Append(extension.ProviderParameterName);
            }
            builder
                .Append(");")
                .NewLine();
        }

        builder.IndentLevel--;
        builder.Indent().Append("});").NewLine();

        builder.EndScope();

        builder.EndScope();
    }

    // ------------------------------------------------------------
    // Helper
    // ------------------------------------------------------------

    private static string MakeFilename(string ns, string className)
    {
        var buffer = new StringBuilder();

        if (!String.IsNullOrEmpty(ns))
        {
            buffer.Append(ns.Replace('.', '_'));
            buffer.Append('_');
        }

        buffer.Append(className);
        buffer.Append(".g.cs");

        return buffer.ToString();
    }
}
