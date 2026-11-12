using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using Unity.Burst;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.Journal.Systems
{
    [UpdateInGroup(typeof(OnTweenStopSystemGroup))]
    public partial struct TweenJournalOnStopSystem : ISystem
    {
        private TweenJournalAccess _tweenJournalAccess;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TweenJournalSingleton>();
            state.RequireForUpdate<TweenJournal>();
            
            state.Enabled = TweenJournalAccess.TryCreate(ref state, out _tweenJournalAccess);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!_tweenJournalAccess.TryGet(ref state, out var singleton))
            {
                return;
            }

            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<TweenOnStop>>()
                         .WithAll<TweenJournal>()
                         .WithEntityAccess())
            {
                singleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                {
                    Entity = entity,
                    Event = TweenJournal.Event.OnStop
                });
            }
            
            _tweenJournalAccess.AddDependency(ref state);
        }
    }
}