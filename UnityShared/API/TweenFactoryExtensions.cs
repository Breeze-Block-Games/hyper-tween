using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.API
{
    public static class TweenFactoryExtensions
    {
        public static TweenFactory<EntityCommandBufferTweenBuilder> CreateTweenFactory(this ref SystemState systemState, bool withJournaling = HyperTweenFactory.JournalingByDefault)
        {
            return systemState.WorldUnmanaged.CreateTweenFactory(withJournaling);
        }
        
        public static TweenFactory<EntityCommandBufferTweenBuilder> CreateTweenFactory(this World world, bool withJournaling = HyperTweenFactory.JournalingByDefault)
        {
            return world.Unmanaged.CreateTweenFactory(withJournaling);
        }

        public static TweenFactory<EntityCommandBufferTweenBuilder> CreateTweenFactory(this WorldUnmanaged world, bool withJournaling = HyperTweenFactory.JournalingByDefault)
        {
            using var entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp);
                
            var query = entityQueryBuilder
                .WithAll<TweenStructuralChangeECBSystem.Singleton>()
                .WithOptions(EntityQueryOptions.IncludeSystems)
                .Build(world.EntityManager);

            var singleton = query.GetSingleton<TweenStructuralChangeECBSystem.Singleton>();
            
            return singleton.GetOrCreateCommandBuffer(world).CreateTweenFactory(world, withJournaling);
        }

        public static TweenFactory<EntityManagerTweenBuilder> CreateTweenFactory(this EntityManager entityManager, bool withJournaling = HyperTweenFactory.JournalingByDefault)
        {
            return new EntityManagerTweenBuilder(entityManager.WorldUnmanaged, entityManager).CreateTweenFactory(withJournaling);
        }

        public static TweenFactory<EntityCommandBufferTweenBuilder> CreateTweenFactory(this EntityCommandBuffer entityCommandBuffer, WorldUnmanaged worldUnmanaged, bool withJournaling = HyperTweenFactory.JournalingByDefault)
        {
            return new EntityCommandBufferTweenBuilder(worldUnmanaged, entityCommandBuffer).CreateTweenFactory(withJournaling);
        }

        public static TweenFactory<EntityCommandBufferParallelWriterTweenBuilder> AsParallelWriter(this TweenFactory<EntityCommandBufferTweenBuilder> tweenFactory)
        {
            return new TweenFactory<EntityCommandBufferParallelWriterTweenBuilder>(tweenFactory.TweenBuilder.AsParallelWriter());
        }
        
        public static TweenFactory<ExclusiveEntityTransactionTweenBuilder> CreateTweenFactory(this ExclusiveEntityTransactionScope exclusiveEntityTransactionScope, bool withJournaling = HyperTweenFactory.JournalingByDefault)
        {
            return exclusiveEntityTransactionScope.GetTweenBuilder().CreateTweenFactory(withJournaling);
        }
                
        public static TweenFactory<T> CreateTweenFactory<T>(this T tweenBuilder, bool withJournaling = HyperTweenFactory.JournalingByDefault) 
            where T : unmanaged, ITweenBuilder
        {
            return new TweenFactory<T>(tweenBuilder, withJournaling);
        }
    }
}