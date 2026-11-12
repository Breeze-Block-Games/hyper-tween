#if HYPER_TWEEN_ENABLE_LIFETIME_VALIDATION
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Validation.Systems
{
    [UpdateInGroup(typeof(TweenStructuralChangeSystemGroup), OrderFirst = true)]
    partial struct TweenLifetimePreValidationSystem : ISystem
    {
        private EntityQuery _playTweensQuery;
        private EntityQuery _stopTweensQuery;

        public void OnCreate(ref SystemState state)
        {
            _playTweensQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenRequestPlaying>()
                .WithNone<TweenPlaying>());
            
            _stopTweensQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenPlaying>()
                .WithNone<TweenRequestPlaying>());
            
            state.EntityManager.CreateSingleton(TweenLifetimeValidationSingleton.Create());
            
            state.RequireForUpdate<TweenLifetimeValidationSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var validationSingleton = SystemAPI.GetSingleton<TweenLifetimeValidationSingleton>();
            
            state.Dependency = validationSingleton.Populate(
                _playTweensQuery, 
                _stopTweensQuery, 
                state.Dependency);
            
            state.CompleteDependency();
        }
    }
}
#endif