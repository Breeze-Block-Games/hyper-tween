using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

// Consumer compiler regressions. Use real Unity reference assemblies from a locally imported project, so malformed output cannot hide behind string assertions.
if (args.Length != 2) throw new ArgumentException("Usage: <Unity project root> <HyperTweenGenerators.dll>");
var root = Path.GetFullPath(args[0]);
var artifact = Path.GetFullPath(args[1]);
var rsp = Directory.GetFiles(Path.Combine(root, "Library/Bee"), "BreezeBlockGames.HyperTween.UnityShared.rsp", SearchOption.AllDirectories).First();
var references = File.ReadAllLines(rsp).Where(x => x.StartsWith("-r:")).Select(x => x[3..].Trim('"'))
    .Select(x => Path.IsPathRooted(x) ? x : Path.Combine(root, x)).ToList();
references.Add(Path.Combine(root, "Library/ScriptAssemblies/BreezeBlockGames.HyperTween.UnityShared.dll"));
var metadata = references.Distinct().Select(x => MetadataReference.CreateFromFile(x)).ToArray();
var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(artifact);
ISourceGenerator Generator(string name) => (ISourceGenerator)Activator.CreateInstance(assembly.GetType("HyperTweenGenerators." + name)!)!;
var failures = 0;
void Check(string name, string source, string generator, Action<Compilation>? assertion = null)
{
    try
    {
        var parse = new CSharpParseOptions(LanguageVersion.CSharp9);
        var input = CSharpCompilation.Create("HyperTween.Consumer." + name,
            new[] { CSharpSyntaxTree.ParseText(source, parse, name + ".cs") }, metadata,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        var inputErrors = input.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error).ToArray();
        if (inputErrors.Length != 0)
            throw new Exception("Invalid consumer: " + string.Join("\n", inputErrors.Select(x => x.ToString())));
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new[] { Generator(generator) }, parseOptions: parse);
        driver = driver.RunGeneratorsAndUpdateCompilation(input, out var generated, out var diagnostics);
        var errors = diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new Exception(string.Join("\n", errors.Select(x => x.ToString())));
        assertion?.Invoke(generated);
        var compiled = generated;
        using var stream = new MemoryStream();
        var result = compiled.Emit(stream);
        var compileErrors = result.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error).ToArray();
        if (!result.Success || compileErrors.Length != 0)
            throw new Exception(string.Join("\n", compileErrors.Select(x => x.ToString())));
        Console.WriteLine("PASS " + name);
    }
    catch (Exception e) { failures++; Console.WriteLine("FAIL " + name + "\n" + e.Message); }
}
Check("MultipleOutputs", """
using Unity.Entities;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
namespace Consumer {
public struct Target : IComponentData { public float Value; }
public struct First : ITweenTo<Target, float> {
 public float GetValue() => 1; public float Lerp(float a, float b, float t) => a+(b-a)*t;
 public float Read(in Target target) => target.Value; public void Write(ref Target target,float value) { target.Value=value; }
}
public struct Second : ITweenTo<Target, float> {
 public float GetValue() => 2; public float Lerp(float a, float b, float t) => a+(b-a)*t;
 public float Read(in Target target) => target.Value; public void Write(ref Target target,float value) { target.Value=value; }
}
}
""", "TweenOutputSystemGenerator", output =>
{
    if (output.GetTypeByMetadataName("BreezeBlockGames.HyperTween.UnityShared.Auto.Components.FirstFrom") == null ||
        output.GetTypeByMetadataName("BreezeBlockGames.HyperTween.UnityShared.Auto.Components.SecondFrom") == null)
        throw new Exception("Both output structs must generate components.");
});
Check("InvocationOrdering", """
using Unity.Entities;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Invoke.Components;
namespace Consumer {
public struct First : ITweenInvokeOnPlay { public void Invoke() {} }
[UpdateAfter(typeof(First)), UpdateBefore(typeof(Last))]
public struct Middle : ITweenInvokeOnPlay { public void Invoke() {} }
public struct Last : ITweenInvokeOnPlay { public void Invoke() {} }
}
""", "TweenInvokeSystemGenerator", output =>
{
    var middle = output.GetTypeByMetadataName("BreezeBlockGames.HyperTween.UnityShared.Auto.Systems.Middle___OnPlay_System") ?? throw new Exception("Missing generated Middle invocation system");
    foreach (var (attributeName, expected) in new[] { ("UpdateAfterAttribute", "First___OnPlay_System"), ("UpdateBeforeAttribute", "Last___OnPlay_System") })
    {
        var value = middle.GetAttributes().SingleOrDefault(x => x.AttributeClass?.Name == attributeName)?.ConstructorArguments[0].Value as ITypeSymbol;
        if (value?.Name != expected)
            throw new Exception(attributeName + " must refer to " + expected + "; actual=" + value?.Name);
    }
});
Console.WriteLine($"{2 - failures}/2 passed ({artifact})");
return failures == 0 ? 0 : 1;
