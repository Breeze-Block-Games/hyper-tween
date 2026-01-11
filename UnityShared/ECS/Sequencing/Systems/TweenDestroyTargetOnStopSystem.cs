using BreezeBlockGames.HyperTween.UnityShared.ECS.Sequencing.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using Unity.Burst;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Sequencing.Systems
{
    [UpdateInGroup(typeof(OnTweenStopSystemGroup))]
    [BurstCompile]
    partial struct TweenDestroyTargetOnStopSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CleanTweenStructuralChangeECBSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<CleanTweenStructuralChangeECBSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var tweenTarget in SystemAPI
                         .Query<RefRO<TweenTarget>>()
                         .WithAll<TweenOnStop, TweenDestroyTargetOnStop>())
            {
                ecb.DestroyEntity(tweenTarget.ValueRO.Target);
            }
        }
    }
}