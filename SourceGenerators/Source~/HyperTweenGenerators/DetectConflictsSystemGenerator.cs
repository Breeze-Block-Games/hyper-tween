using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unity.Entities.SourceGen.Common;

namespace HyperTweenGenerators;

[Generator]
public class DetectConflictsSystemGenerator : ISourceGenerator
{
    private const string TweenToSymbolName = "BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components.ITweenTo";
    private const string AutoDetectConflictsSymbolName = "BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Attributes.DetectConflictsAttribute";

    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (!SourceGenHelpers.IsBuildTime || !SourceGenHelpers.ShouldRun(context.Compilation, context.CancellationToken))
            return;

        SourceOutputHelpers.Setup(context.ParseOptions, context.AdditionalFiles);

        Location? lastLocation = null;
        SourceOutputHelpers.LogInfoToSourceGenLog($"Source generating assembly {context.Compilation.Assembly.Name}...");

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var compilation = context.Compilation;
            var syntaxTrees = compilation.SyntaxTrees;
            
            var autoDetectConflictsStructs = new List<(ITypeSymbol,ITypeSymbol?)>();

            foreach (var syntaxTree in syntaxTrees)
            {
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                
                var structDeclarationSyntaxes = syntaxTree.GetRoot()
                    .DescendantNodes()
                    .OfType<StructDeclarationSyntax>();

                foreach (var structDeclaration in structDeclarationSyntaxes)
                {
                    var structSymbol = semanticModel.GetDeclaredSymbol(structDeclaration) as ITypeSymbol;

                    if (structSymbol == null)
                    {
                        continue;
                    }

                    if (!structSymbol.ImplementsInterface(TweenToSymbolName))
                    {
                        continue;
                    }

                    var autoDetectConflictsAttributeData = structSymbol.GetAttributeData(AutoDetectConflictsSymbolName);
                    if (autoDetectConflictsAttributeData == null)
                    {
                        continue;
                    }
                    
                    var instanceIdComponentTypedConstant = autoDetectConflictsAttributeData.ConstructorArguments
                        .FirstOrDefault();
                    
                    // TODO: Validate that instanceIdComponentType only contains an int, and is an IComponent
                    
                    autoDetectConflictsStructs.Add((structSymbol, instanceIdComponentTypedConstant.Kind != TypedConstantKind.Error ? instanceIdComponentTypedConstant.GetExpressedType() : default(ITypeSymbol)));
                }
            }

            var generatedPartials = GenerateDetectConflictsHelper(context.Compilation.AssemblyName!, autoDetectConflictsStructs);
            
            foreach (var tuple in generatedPartials)
            {
                var (hint, source) = tuple;
                context.AddSource($"{hint}.g.cs", source);
                
                SourceOutputHelpers.OutputSourceToFile(
                    $"Temp/GeneratedCode/{context.Compilation.Assembly.Name}/{hint}.g.cs",
                    () => source);
            }

            stopwatch.Stop();

            SourceOutputHelpers.LogInfoToSourceGenLog($"TIME : {nameof(DetectConflictsSystemGenerator)} : {context.Compilation.Assembly.Name} : {stopwatch.ElapsedMilliseconds}ms");
        }
        catch (Exception exception)
        {
            if (exception is OperationCanceledException)
                throw;

            context.LogError("SGICE002", nameof(DetectConflictsSystemGenerator), exception.ToUnityPrintableString(), lastLocation ?? context.Compilation.SyntaxTrees.First().GetRoot().GetLocation());
        }
    }

    private IEnumerable<(string,string)> GenerateDetectConflictsHelper(string assemblyName, List<(ITypeSymbol, ITypeSymbol?)> conflictTypeSymbols)
    {
        var componentNames = conflictTypeSymbols
            .Select(tuple => (tuple.Item1.ToFullName(), tuple.Item2?.ToFullName()))
            .ToArray();

        if (componentNames.Length == 0)
        {
            yield break;
        }

        var dynamicTypeInfoDefinitions = string.Join("\n", componentNames
            .Select( (_,i) => $"[ReadOnly] public TweenComponentMetaData TweenComponentMetaData{i};")
            .ToArray());
        
        var initDynamicTypeInfos = string.Join("\n", componentNames
            .Select( (tuple,i) =>
            {
                if (tuple.Item2 != null)
                {
                    return $"TweenComponentMetaData{i}.Initialise<{tuple.Item1}, {tuple.Item2}>(ref state);";
                }

                return $"TweenComponentMetaData{i}.Initialise<{tuple.Item1}>(ref state);";
            })
            .ToArray());

        var updateDynamicTypeInfos = string.Join("\n", componentNames
            .Select( (_,i) => $"TweenComponentMetaData{i}.Update(ref state);")
            .ToArray());
        
        var componentTypes = string.Join(",\n", componentNames
            .Select(tuple => $"ComponentType.ReadOnly(typeof({tuple.Item1}))")
            .ToArray());

        yield return ("TweenComponentMetaDataContainer"
            ,$$"""
             using Unity.Entities;
             using Unity.Collections;
             using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Systems;
             
             namespace {{assemblyName}}.Auto
             {
                 [global::System.Runtime.CompilerServices.CompilerGenerated]
                 public struct TweenComponentsMetaDataContainer
                 {
                    {{dynamicTypeInfoDefinitions}}

                    public void Initialise(ref SystemState state)
                    {
                        {{initDynamicTypeInfos}}
                    }
                    
                    public void Update(ref SystemState state)
                    {
                        {{updateDynamicTypeInfos}}
                    }
                    
                    public static NativeList<ComponentType> GetTweenComponentTypes()
                    {
                          return new NativeList<ComponentType>({{componentNames.Length}}, Allocator.Persistent)
                          {
                              {{componentTypes}}
                          };
                    }
                 }
             }
             """);

        yield return ("ConflictLookup",
            $$"""
              using System;
              using Unity.Entities;
              using Unity.Collections;
              using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Systems;
              
              namespace {{assemblyName}}.Auto
              {
                  public struct ConflictLookup : IComponentData, IDisposable
                  {
                      public NativeHashMap<EntityTypeKey, Entity> EntityTypeKeyToTweenMap;
                      public NativeHashMap<InstanceIdKey, Entity> GameObjectTypeKeyToTweenMap;
                  
                      public static ConflictLookup Allocate()
                      {
                          return new ConflictLookup()
                          {
                              EntityTypeKeyToTweenMap = new NativeHashMap<EntityTypeKey, Entity>(8, Allocator.Persistent),
                              GameObjectTypeKeyToTweenMap = new NativeHashMap<InstanceIdKey, Entity>(8, Allocator.Persistent)
                          };
                      }
                      
                      public void Dispose()
                      {
                          EntityTypeKeyToTweenMap.Dispose();
                          GameObjectTypeKeyToTweenMap.Dispose();
                      }
                  }
              }
              """
        );
        
        yield return ("AddToConflictLookupSystem",
            $$"""
            using Unity.Burst;
            using Unity.Collections;
            using Unity.Collections.LowLevel.Unsafe;
            using Unity.Entities;
            using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Components;
            using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Systems;
            using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
            using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems;
            using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
            using BreezeBlockGames.HyperTween.UnityShared.ECS.Util;
            using BreezeBlockGames.HyperTween.UnityShared.Journal.Systems;
            using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
            
            namespace {{assemblyName}}.Auto
            {
                [global::System.Runtime.CompilerServices.CompilerGenerated]
                [UpdateInGroup(typeof(OnTweenPlaySystemGroup))]
                [BurstCompile]
                public partial struct AddToConflictLookupSystem : ISystem
                {
                    private EntityQuery _onPlayQuery, _ecbSingletonQuery, _conflictLookupQuery;
                    private TweenComponentsMetaDataContainer _tweenComponentsMetaDataContainer;
                    private FieldEnumerable<TweenComponentsMetaDataContainer,TweenComponentMetaData> _tweenComponentsMetaDataEnumerable;
                    private ComponentTypeHandle<TweenJournal> _tweenJournalTypeHandle;
                    private TweenJournalAccess _tweenJournalAccess;
            
                    private ComponentTypeHandle<TweenTarget> _tweenTargetTypeHandle;
                    private EntityTypeHandle _entityTypeHandle;
                    
                    public void OnCreate(ref SystemState state)
                    {
                        _ecbSingletonQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                            .WithAll<TweenStructuralChangeECBSystem.Singleton>()
                            .WithOptions(EntityQueryOptions.IncludeSystems));
                            
                        _conflictLookupQuery = state.GetEntityQuery(ComponentType.ReadWrite<ConflictLookup>());
                        
                        var componentTypes = TweenComponentsMetaDataContainer.GetTweenComponentTypes();
                
                        _onPlayQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                            .WithAny(ref componentTypes)
                            .WithAll<TweenOnPlay>());
                
                        _tweenComponentsMetaDataEnumerable =
                            new FieldEnumerable<TweenComponentsMetaDataContainer, TweenComponentMetaData>(Allocator.Persistent);
                
                        _tweenComponentsMetaDataContainer.Initialise(ref state);
                        
                        state.RequireForUpdate(_onPlayQuery);
                        state.RequireForUpdate<TweenStructuralChangeECBSystem.Singleton>();
                        
                        _tweenJournalTypeHandle = state.GetComponentTypeHandle<TweenJournal>(true);
                        
                        componentTypes.Dispose();
                        
                        TweenJournalAccess.TryCreate(ref state, out _tweenJournalAccess);
                        
                        _tweenTargetTypeHandle = state.GetComponentTypeHandle<TweenTarget>(true);
                        _entityTypeHandle = state.GetEntityTypeHandle();
                    }
                    
                    [BurstCompile]
                    public void OnDestroy(ref SystemState state)
                    {
                        _tweenComponentsMetaDataEnumerable.Dispose();
                    }
                
                    [BurstCompile]
                    public void OnUpdate(ref SystemState state)
                    {
                        var ecbSingleton = _ecbSingletonQuery.GetSingleton<TweenStructuralChangeECBSystem.Singleton>();
                        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
                
                        var conflictLookup = _conflictLookupQuery.GetSingletonRW<ConflictLookup>();
                        
                        _tweenComponentsMetaDataContainer.Update(ref state);
                
            #if HYPER_TWEEN_ENABLE_JOURNAL
                        _tweenJournalAccess.TryGet(ref state, out var tweenJournalSingleton);
            #endif
            
                        _tweenJournalTypeHandle.Update(ref state);
                        _tweenTargetTypeHandle.Update(ref state);
                        _entityTypeHandle.Update(ref state);
                        
                        state.Dependency = new AddToConflictLookupSystemJob<TweenComponentsMetaDataContainer>()
                        {
                            EntityTypeKeyToTweenMap = conflictLookup.ValueRW.EntityTypeKeyToTweenMap,
                            InstanceIdToTweenMap = conflictLookup.ValueRW.GameObjectTypeKeyToTweenMap,
                            TweenTargetTypeHandle = _tweenTargetTypeHandle,
                            MetaDataContainer = _tweenComponentsMetaDataContainer,
                            TweenComponentsMetaDataEnumerable = _tweenComponentsMetaDataEnumerable,
                            EntityTypeHandle = _entityTypeHandle,
                            EntityCommandBuffer = ecb,
                            IntSize = UnsafeUtility.SizeOf<int>(),
            #if HYPER_TWEEN_ENABLE_JOURNAL
                            TweenJournalTypeHandle = _tweenJournalTypeHandle,
                            TweenJournalSingleton = tweenJournalSingleton.ValueRW
            #endif
                        }.Schedule(_onPlayQuery, state.Dependency);
                        
                        _tweenJournalAccess.AddDependency(ref state);
                    }
                }
            }
            """);
        
        yield return ("RemoveFromConflictLookupSystem",
            $$"""
            using Unity.Burst;
            using Unity.Collections;
            using Unity.Collections.LowLevel.Unsafe;
            using Unity.Entities;
            using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Components;
            using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Systems;
            using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
            using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems;
            using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
            using BreezeBlockGames.HyperTween.UnityShared.ECS.Util;
            
            namespace {{assemblyName}}.Auto
            {
                [global::System.Runtime.CompilerServices.CompilerGenerated]
                [UpdateInGroup(typeof(OnTweenStopSystemGroup))]
                [BurstCompile]
                public partial struct RemoveFromConflictLookupSystem : ISystem
                {
                    private EntityQuery _onStopQuery, _conflictLookupQuery;
                    private TweenComponentsMetaDataContainer _tweenComponentsMetaData;
                    private FieldEnumerable<TweenComponentsMetaDataContainer, TweenComponentMetaData> _tweenComponentsMetaDataEnumerable;
                    private NativeList<ComponentType> _componentTypes;
            
                    private ComponentTypeHandle<TweenTarget> _tweenTargetTypeHandle;
                    private EntityTypeHandle _entityTypeHandle;
            
                    public void OnCreate(ref SystemState state)
                    {
                        state.EntityManager.CreateSingleton(ConflictLookup.Allocate());
                
                        _componentTypes = TweenComponentsMetaDataContainer.GetTweenComponentTypes();
                
                        _onStopQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                            .WithAny(ref _componentTypes)
                            .WithAll<TweenOnStop>()
                            // Don't remove Tweens that have conflicted - we know the target has been taken over by another entity
                            .WithNone<TweenConflicted>());
                            
                        _conflictLookupQuery = state.GetEntityQuery(ComponentType.ReadWrite<ConflictLookup>());
            
                        _tweenComponentsMetaDataEnumerable =
                            new FieldEnumerable<TweenComponentsMetaDataContainer, TweenComponentMetaData>(Allocator.Persistent);
                
                        _tweenComponentsMetaData.Initialise(ref state);
                        
                        _tweenTargetTypeHandle = state.GetComponentTypeHandle<TweenTarget>();
                        _entityTypeHandle = state.GetEntityTypeHandle();
                        
                        state.RequireForUpdate(_onStopQuery);
                    }
                
                    public void OnDestroy(ref SystemState state)
                    {
                        _tweenComponentsMetaDataEnumerable.Dispose();
                        
                        ref var conflictLookup = ref _conflictLookupQuery.GetSingletonRW<ConflictLookup>().ValueRW;
                        conflictLookup.Dispose();
                
                        _componentTypes.Dispose();
                    }
                    
                    [BurstCompile]
                    public void OnUpdate(ref SystemState state)
                    {
                        var conflictLookup = _conflictLookupQuery.GetSingletonRW<ConflictLookup>();
                        _tweenComponentsMetaData.Update(ref state);
                        
                        _tweenTargetTypeHandle.Update(ref state);
                        _entityTypeHandle.Update(ref state);
            
                        state.Dependency = new RemoveFromConflictLookupJob<TweenComponentsMetaDataContainer>()
                        {
                            TargetToTweenMap = conflictLookup.ValueRW.EntityTypeKeyToTweenMap,
                            GameObjectTypeKeyToTweenMap = conflictLookup.ValueRW.GameObjectTypeKeyToTweenMap,
                            TweenTargetTypeHandle = _tweenTargetTypeHandle,
                            MetaDataContainer = _tweenComponentsMetaData,
                            TweenComponentsMetaDataEnumerable = _tweenComponentsMetaDataEnumerable,
                            EntityTypeHandle = _entityTypeHandle,
                            IntSize = UnsafeUtility.SizeOf<int>()
                        }.Schedule(_onStopQuery, state.Dependency);
                    }
                }
            }
            """);
    }
}