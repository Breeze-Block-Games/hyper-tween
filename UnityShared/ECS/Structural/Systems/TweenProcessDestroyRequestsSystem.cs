using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    partial struct TweenProcessDestroyRequestsSystem : ISystem
    {
        private EntityQuery _query;
        private TweenJournalAccess _tweenJournalAccess;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            
            _query = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenRequestDestroy>());
            
            state.RequireForUpdate(_query);
            
            TweenJournalAccess.TryCreate(ref state, out _tweenJournalAccess);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

            ecb.DestroyEntity(_query, EntityQueryCaptureMode.AtPlayback);
            
            if (!_tweenJournalAccess.TryGet(ref state, out var singleton))
            {
                return;
            }

            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<TweenRequestDestroy>>()
                         .WithAll<TweenJournal>()
                         .WithEntityAccess())
            {
                // Last chance to add the name
                if (!singleton.ValueRW.NameLookup.ContainsKey(entity))
                {
                    state.EntityManager.GetName(entity, out var name);
                    singleton.ValueRW.NameLookup.Add(entity, name);
                }
                
                singleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                {
                    Entity = entity,
                    Event = TweenJournal.Event.Destroy
                });
            }
            
            _tweenJournalAccess.AddDependency(ref state);
        }
    }
}