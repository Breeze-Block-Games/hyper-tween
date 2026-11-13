using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace HyperTweenGenerators;

// Keep the analyzer self-contained: Unity's analyzer loader does not resolve helpers
// from another package's analyzer directory.
internal static class SourceGenHelpers
{
    public static bool IsBuildTime => Assembly.GetEntryAssembly() != null;

    public static bool ShouldRun(Compilation compilation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return (compilation.AssemblyName == "Unity.Entities" ||
                compilation.ReferencedAssemblyNames.Any(assembly => assembly.Name == "Unity.Entities")) &&
               !(compilation.AssemblyName?.Contains("CodeGen.Tests") ?? false);
    }

    public static void LogError(this GeneratorExecutionContext context, string id, string title,
        string message, Location location)
    {
        var descriptor = new DiagnosticDescriptor(id, title, "{0}", "Source Generator",
            DiagnosticSeverity.Error, isEnabledByDefault: true);
        context.ReportDiagnostic(Diagnostic.Create(descriptor, location, message));
    }

    public static string ToUnityPrintableString(this Exception exception) =>
        exception.ToString().Replace(Environment.NewLine, " |--| ");

    public static string ToFullName(this ISymbol symbol) =>
        symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    public static bool HasAttribute(this ISymbol symbol, string name)
    {
        var qualifiedName = name.StartsWith("global::", StringComparison.Ordinal) ? name : "global::" + name;
        return symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.ToFullName() == qualifiedName);
    }
}

internal sealed class SourceOutputHelpers
{
    private readonly string? _projectPath;

    public SourceOutputHelpers(GeneratorExecutionContext context)
    {
        if (Environment.GetEnvironmentVariable("SOURCEGEN_DISABLE_PROJECT_PATH_OUTPUT") != "1" &&
            context.ParseOptions.PreprocessorSymbolNames.Contains("DOTS_OUTPUT_SOURCEGEN_FILES") &&
            !context.AdditionalFiles.IsDefaultOrEmpty)
        {
            _projectPath = context.AdditionalFiles[0].GetText(context.CancellationToken)?.ToString();
        }
    }

    public (string FullFilePath, string FileNameOnly) GetGeneratedSourceFilePath(
        SyntaxTree tree, string assemblyName, string generatorName, int salting)
    {
        var fileName = $"{Path.GetFileNameWithoutExtension(tree.FilePath)}__{generatorName}_{StableHash(tree.FilePath) & int.MaxValue}{salting}.g.cs";
        var directory = $"Temp/GeneratedCode/{assemblyName}";
        if (!string.IsNullOrEmpty(_projectPath))
            directory = Path.Combine(_projectPath, directory);
        return ($"{directory}/{fileName}".Replace('\\', '/'), fileName);
    }

    // Preserve existing hint names across processes and runtimes.
    private static int StableHash(string value)
    {
        unchecked
        {
            int even = 5381, odd = 5381;
            for (var i = 0; i < value.Length && value[i] != '\0'; i++)
            {
                if ((i & 1) == 0)
                    even = even * 33 ^ value[i];
                else
                    odd = odd * 33 ^ value[i];
            }
            return even + odd * 1566083941;
        }
    }

    public void OutputSourceToFile(string path, Func<string> source)
    {
        if (string.IsNullOrEmpty(_projectPath))
            return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, source());
        }
        catch (IOException exception)
        {
            LogInfoToSourceGenLog(exception.ToUnityPrintableString());
        }
    }

    public void LogInfoToSourceGenLog(string message)
    {
        if (string.IsNullOrEmpty(_projectPath))
            return;
        try
        {
            var directory = Path.Combine(_projectPath, "Temp", "GeneratedCode");
            Directory.CreateDirectory(directory);
            File.AppendAllText(Path.Combine(directory, "SourceGen.log"), message + Environment.NewLine);
        }
        catch (IOException)
        {
            // Optional debug output must not prevent compilation.
        }
    }
}

internal static class TypeCreationHelpers
{
    public static SourceText FixUpLineDirectivesAndOutputSource(string path, string source) =>
        SourceText.From("#pragma warning disable 0618" + Environment.NewLine +
                        "#pragma warning disable 0219" + Environment.NewLine +
                        $"#line 1 \"{path}\"" + Environment.NewLine + source, Encoding.UTF8);
}
