using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.Journal.Systems
{
    public struct TweenJournalAccess
    {
        private EntityQuery _entityQuery;
        
        public static bool TryCreate(EntityManager entityManager, out TweenJournalAccess tweenJournalAccess)
        {
#if !HYPER_TWEEN_ENABLE_JOURNAL
            tweenJournalAccess = default;
            return false;
#endif
            
            tweenJournalAccess = new TweenJournalAccess()
            {
                _entityQuery = new EntityQueryBuilder(Allocator.Temp)
                    .WithAllRW<TweenJournalSingleton>()
                    .Build(entityManager)
            };

            return true;
        }
        
        public static bool TryCreate(ref SystemState systemState, out TweenJournalAccess tweenJournalAccess)
        {
#if !HYPER_TWEEN_ENABLE_JOURNAL
            tweenJournalAccess = default;
            return false;
#endif
            
            tweenJournalAccess = new TweenJournalAccess()
            {
                _entityQuery = new EntityQueryBuilder(Allocator.Temp)
                    .WithAllRW<TweenJournalSingleton>()
                    .Build(ref systemState)
            };

            return true;
        }
        
        public bool TryGet(out RefRW<TweenJournalSingleton> tweenJournalSingleton)
        {
#if !HYPER_TWEEN_ENABLE_JOURNAL
            tweenJournalSingleton = default;
            return false;
#endif
            
            // TODO: There must be a better way to do this that doesn't block...
            _entityQuery.GetDependency().Complete();
            
            return _entityQuery.TryGetSingletonRW(out tweenJournalSingleton);
        }
        
        public bool TryGet(ref SystemState systemState, out RefRW<TweenJournalSingleton> tweenJournalSingleton)
        {
#if !HYPER_TWEEN_ENABLE_JOURNAL
            tweenJournalSingleton = default;
            return false;
#endif
            
            // TODO: There must be a better way to do this that doesn't block...
            systemState.EntityManager.CompleteDependencyBeforeRW<TweenJournalSingleton>();
            
            return _entityQuery.TryGetSingletonRW(out tweenJournalSingleton);
        }

        public void AddDependency(ref SystemState systemState)
        {
#if !HYPER_TWEEN_ENABLE_JOURNAL
            return;
#endif
            
            systemState.Dependency = _entityQuery.AddDependency(systemState.Dependency);
        }
    }
}