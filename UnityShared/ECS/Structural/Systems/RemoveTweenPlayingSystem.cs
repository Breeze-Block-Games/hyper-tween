using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems
{
    [UpdateInGroup(typeof(TweenStructuralChangeSystemGroup))]
    [UpdateAfter(typeof(OnTweenStopSystemGroup))]
    [UpdateBefore(typeof(CleanTweenStructuralChangeECBSystem))]
    [BurstCompile]
    partial struct RemoveTweenPlayingSystem : ISystem
    {
        private EntityQuery _nonConflictedQuery;
        private EntityQuery _conflictedQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CleanTweenStructuralChangeECBSystem.Singleton>();
            
            _nonConflictedQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenOnStop>());
            
            _conflictedQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenOnStop, TweenConflicted>());
            
            state.RequireAnyForUpdate(_nonConflictedQuery, _conflictedQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<CleanTweenStructuralChangeECBSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

            ecb.RemoveComponent<TweenPlaying>(_nonConflictedQuery, EntityQueryCaptureMode.AtPlayback);
            ecb.RemoveComponent<TweenConflicted>(_conflictedQuery, EntityQueryCaptureMode.AtPlayback);
        }
    }
}