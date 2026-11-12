using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems
{
    [UpdateInGroup(typeof(TweenStructuralChangeSystemGroup))]
    [UpdateBefore(typeof(OnTweenPlaySystemGroup))]
    [UpdateBefore(typeof(OnTweenStopSystemGroup))]
    partial struct AddTweenOnPlayStopSystem : ISystem
    {
        private EntityQuery _toPlayQuery, _onPlayQuery;
        private EntityQuery _toStopQuery, _onStopQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CleanTweenStructuralChangeECBSystem.Singleton>();

            _toPlayQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenRequestPlaying>()
                .WithNone<TweenPlaying, TweenRequestDestroy>());
            
            _onPlayQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenOnPlay>());
            
            _toStopQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenPlaying>()
                .WithNone<TweenRequestPlaying, TweenRequestDestroy>());
            
            _onStopQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenOnStop>());
            
            state.RequireAnyForUpdate(_toPlayQuery, _onPlayQuery, _toStopQuery, _onStopQuery);
        }

        public void OnUpdate(ref SystemState state)
        {
            state.EntityManager.RemoveComponent<TweenOnPlay>(_onPlayQuery);
            state.EntityManager.RemoveComponent<TweenOnStop>(_onStopQuery);
            
            state.EntityManager.AddComponent<TweenOnPlay>(_toPlayQuery);
            state.EntityManager.AddComponent<TweenOnStop>(_toStopQuery);
        }
    }
}