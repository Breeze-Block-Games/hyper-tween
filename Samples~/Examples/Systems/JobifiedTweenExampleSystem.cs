using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.API;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
public partial struct JobifiedTweenExampleSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystem = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
    
        var tweenFactory = state.CreateTweenFactory().AsParallelWriter();
        var random = new Random(1);
        
        foreach (var (_, entity) in SystemAPI.Query<RefRO<TestComponentB>>().WithEntityAccess())
        {
            tweenFactory.CreateTween("SystemStateTweenExampleSystem")
                .WithTarget(entity)
                .WithDuration(1f)
                .WithLocalPositionOutput(to: -5f + (10f * random.NextFloat3()))
                .Play();

            ecb.RemoveComponent<TestComponentB>(entity);
        }
    }
}