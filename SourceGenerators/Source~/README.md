# HyperTween source generators

HyperTween generates conflict detection, tween output, and invocation components and systems. The analyzer is self-contained and does not require an Entities PackageCache project or a separately shipped `Common.dll`.

## Build

Install the .NET 8 SDK or newer; the generator source uses C# 12 features. From the HyperTween package root, run:

```sh
dotnet publish SourceGenerators/Source~/HyperTweenGenerators/HyperTweenGenerators.csproj -c Release
```

The project targets `netstandard2.0` and restores its Roslyn build dependency through NuGet. Publishing rebuilds `SourceGenerators/HyperTweenGenerators.dll` and its PDB in place. Preserve their existing `.meta` files and analyzer labels. The build has also been verified with the .NET 9 SDK.

To build debug symbols with debug optimization settings:

```sh
dotnet publish SourceGenerators/Source~/HyperTweenGenerators/HyperTweenGenerators.csproj -c Debug
```

## Inspect generated code

Add `DOTS_OUTPUT_SOURCEGEN_FILES` to the Unity project's scripting define symbols and recompile. When Unity supplies its project-path additional file, the generators write copies under `Temp/GeneratedCode/<AssemblyName>/` and append diagnostic information to `Temp/GeneratedCode/SourceGen.log` in the Unity project. `SOURCEGEN_DISABLE_PROJECT_PATH_OUTPUT=1` disables this optional file output; generated sources still participate in compilation.

The package-owned output and Roslyn helpers are in `SourceGenerators/Source~/HyperTweenGenerators/GeneratorHelpers.cs`. The solution at `SourceGenerators/Source~/SourceGenerators.sln` can be opened from the package root for source debugging. To reproduce a Unity compilation outside the editor, use the compiler command and response files recorded in that project's editor log, with the Unity project root as the working directory.
