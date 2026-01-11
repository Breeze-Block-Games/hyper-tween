using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.API;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public partial struct BatchTweenExampleSystem : ISystem
{
    [BurstCompile]
    private struct CreateIndirectTweensJob : IJob
    {
        public TweenFactory<EntityCommandBufferTweenBuilder> TweenFactory;
            
        public float Duration;
            
        [ReadOnly]
        public NativeArray<Entity> Targets;

        [BurstCompile]
        public void Execute()
        {
            var positions = new NativeArray<float3>(Targets.Length, Allocator.Temp);

            var random = new Random(1);
            for (var i = 0; i < positions.Length; i++)
            {
                positions[i] = -5f + (10f * random.NextFloat3());
            }

            using var batchTweenHandle = TweenFactory.CreateTween(nameof(BatchTweenExampleSystem))
                .WithDuration(Duration)
                .Play()
                .CreateBatch(Targets.Length, Allocator.Temp)
                .WithTargets(Targets)
                .WithLocalPositionOutputs(positions);
            
            positions.Dispose();
        }
    }
    
    private EntityQuery _entityQuery;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();

        _entityQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<TestComponentA>()
            .Build(ref state);
        
        state.RequireForUpdate<TestComponentA>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
    
        using var entities = _entityQuery.ToEntityArray(Allocator.TempJob);
        
        new CreateIndirectTweensJob()
        { 
            TweenFactory = ecb.CreateTweenFactory(state.WorldUnmanaged),
            Duration = 5f,
            Targets = entities
        }.Run();
        
        ecb.RemoveComponent<TestComponentA>(_entityQuery, EntityQueryCaptureMode.AtPlayback);
    }
}