namespace AutoMapperFragmentConfig.SourceGenerator;

using System;
using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public class Generator : IIncrementalGenerator
{
    // ------------------------------------------------------------
    // Generator
    // ------------------------------------------------------------

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compilationProvider = context.CompilationProvider;
        var mapConfigs = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => IsMapConfigTargetSyntax(node),
                static (context, _) => GetMapConfigTargetSyntax(context))
            .SelectMany(static (x, _) => x is not null ? ImmutableArray.Create(x) : [])
            .Collect();
        var mapExtensions = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => IsMapExtensionTargetSyntax(node),
                static (context, _) => GetMapExtensionTargetSyntax(context))
            .SelectMany(static (x, _) => x is not null ? ImmutableArray.Create(x) : [])
            .Collect();

        var providers = compilationProvider.Combine(mapConfigs.Combine(mapExtensions));

        context.RegisterImplementationSourceOutput(
            providers,
            static (spc, source) => Execute(spc, source.Left, source.Right.Left, source.Right.Right));
    }

    private static bool IsMapConfigTargetSyntax(SyntaxNode node) =>
        node is MethodDeclarationSyntax { AttributeLists.Count: > 0 };

    private static MethodDeclarationSyntax? GetMapConfigTargetSyntax(GeneratorSyntaxContext context)
    {
        var methodDeclarationSyntax = (MethodDeclarationSyntax)context.Node;
        if ((methodDeclarationSyntax.ParameterList.Parameters.Count != 1) &&
            (methodDeclarationSyntax.ParameterList.Parameters.Count != 2))
        {
            return null;
        }

        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
        if ((methodSymbol is null) || !methodSymbol.IsStatic)
        {
            return null;
        }

        var attribute = methodSymbol.GetAttributes()
            .FirstOrDefault(static x => x.AttributeClass!.ToDisplayString() == "AutoMapperFragmentConfig.MapConfigAttribute");
        if (attribute is null)
        {
            return null;
        }

        if (methodSymbol.Parameters[0].Type.ToDisplayString() != "AutoMapper.IProfileExpression")
        {
            return null;
        }

        if ((methodSymbol.Parameters.Length > 1) &&
            (methodSymbol.Parameters[1].Type.ToDisplayString() != "System.IServiceProvider"))
        {
            return null;
        }

        return methodDeclarationSyntax;
    }

    private static bool IsMapExtensionTargetSyntax(SyntaxNode node) =>
        node is MethodDeclarationSyntax { AttributeLists.Count: > 0 };

    private static MethodDeclarationSyntax? GetMapExtensionTargetSyntax(GeneratorSyntaxContext context)
    {
        var methodDeclarationSyntax = (MethodDeclarationSyntax)context.Node;
        if ((methodDeclarationSyntax.ParameterList.Parameters.Count != 1) &&
            (methodDeclarationSyntax.ParameterList.Parameters.Count != 2))
        {
            return null;
        }

        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
        if ((methodSymbol is null) || !methodSymbol.IsPartialDefinition || !methodSymbol.IsExtensionMethod || !methodSymbol.IsStatic || !methodSymbol.ReturnsVoid)
        {
            return null;
        }

        var attribute = methodSymbol.GetAttributes()
            .FirstOrDefault(static x => x.AttributeClass!.ToDisplayString() == "AutoMapperFragmentConfig.MapConfigExtensionAttribute");
        if (attribute is null)
        {
            return null;
        }

        if (methodSymbol.Parameters[0].Type.ToDisplayString() != "AutoMapper.IMapperConfigurationExpression")
        {
            return null;
        }

        if ((methodSymbol.Parameters.Length > 1) &&
            (methodSymbol.Parameters[1].Type.ToDisplayString() != "System.IServiceProvider"))
        {
            return null;
        }

        return methodDeclarationSyntax;
    }

    // ------------------------------------------------------------
    // Builder
    // ------------------------------------------------------------

    private static void Execute(SourceProductionContext context, Compilation compilation, ImmutableArray<MethodDeclarationSyntax> mapConfigs, ImmutableArray<MethodDeclarationSyntax> mapExtensions)
    {
        var filename = new StringBuilder();
        var source = new StringBuilder();

        foreach (var extensionMethodDeclarationSyntax in mapExtensions)
        {
            // Check cancel
            context.CancellationToken.ThrowIfCancellationRequested();

            // Build metadata
            var methodSemantic = compilation.GetSemanticModel(extensionMethodDeclarationSyntax.SyntaxTree);
            var methodSymbol = methodSemantic.GetDeclaredSymbol(extensionMethodDeclarationSyntax)!;

            var classSymbol = methodSymbol.ContainingType;
            var ns = classSymbol.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : classSymbol.ContainingNamespace.ToDisplayString();
            var className = classSymbol.Name;

            var attribute = methodSymbol.GetAttributes()
                .First(static x => x.AttributeClass!.ToDisplayString() == "AutoMapperFragmentConfig.MapConfigExtensionAttribute");
            var profileName = attribute.ConstructorArguments.Length > 0
                ? attribute.ConstructorArguments[0].Value!.ToString()
                : "_Fragment";

            // Source
            source.AppendLine("// <auto-generated />");
            source.AppendLine("#nullable disable");

            // namespace
            if (!String.IsNullOrEmpty(ns))
            {
                source.Append("namespace ").Append(ns).AppendLine();
            }

            source.AppendLine("{");

            // class
            source.Append("    partial ").Append(classSymbol.IsValueType ? "struct " : "class ").Append(className).AppendLine();
            source.AppendLine("    {");

            // method
            source.Append("        ");
            source.Append(ToAccessibilityText(methodSymbol.DeclaredAccessibility));
            source.Append(" static partial void ");
            source.Append(methodSymbol.Name);
            source.Append("(this ");
            source.Append(String.Join(", ", methodSymbol.Parameters.Select(static x => $"{x.Type.ToDisplayString()} {x.Name}")));
            source.Append(')');
            source.AppendLine();

            source.AppendLine("        {");

            source.Append("            ");
            source.Append(methodSymbol.Parameters[0].Name);
            source.Append('.');
            source.Append("CreateProfile(");
            source.Append('"');
            source.Append(profileName);
            source.Append('"');
            source.Append(", x =>");
            source.AppendLine();

            source.AppendLine("            {");

            foreach (var mapMethodDeclarationSyntax in mapConfigs)
            {
                AddConfigEntry(compilation, source, mapMethodDeclarationSyntax, methodSymbol.Parameters);
            }

            source.AppendLine("            });");
            source.AppendLine("        }");

            source.AppendLine("    }");
            source.AppendLine("}");

            // Write
            context.AddSource(
                MakeFilename(filename, ns, className),
                SourceText.From(source.ToString(), Encoding.UTF8));

            source.Clear();
        }
    }

    private static void AddConfigEntry(Compilation compilation, StringBuilder source, MethodDeclarationSyntax methodDeclarationSyntax, ImmutableArray<IParameterSymbol> parameters)
    {
        var methodSemantic = compilation.GetSemanticModel(methodDeclarationSyntax.SyntaxTree);
        var methodSymbol = methodSemantic.GetDeclaredSymbol(methodDeclarationSyntax)!;

        source.Append("                ");
        source.Append(methodSymbol.ContainingType.ToDisplayString());
        source.Append('.');
        source.Append(methodSymbol.Name);
        source.Append('(');
        source.Append(parameters[0].Name);
        if ((parameters.Length > 1) && (methodSymbol.Parameters.Length > 1))
        {
            source.Append(", ");
            source.Append(parameters[1].Name);
        }
        source.Append(");");
        source.AppendLine();
    }

    private static string ToAccessibilityText(Accessibility accessibility) => accessibility switch
    {
        Accessibility.Public => "public",
        Accessibility.Protected => "protected",
        Accessibility.Private => "private",
        Accessibility.Internal => "internal",
        Accessibility.ProtectedOrInternal => "protected internal",
        Accessibility.ProtectedAndInternal => "private protected",
        _ => throw new NotSupportedException()
    };

    private static string MakeFilename(StringBuilder buffer, string ns, string className)
    {
        buffer.Clear();

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
