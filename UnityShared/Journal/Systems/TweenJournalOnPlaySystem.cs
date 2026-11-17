using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using Unity.Burst;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.Journal.Systems
{
    [UpdateInGroup(typeof(OnTweenPlaySystemGroup))]
    public partial struct TweenJournalOnPlaySystem : ISystem
    {
        private TweenJournalAccess _tweenJournalAccess;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.Enabled = TweenJournalAccess.TryCreate(ref state, out _tweenJournalAccess);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!_tweenJournalAccess.TryGet(out var singleton))
            {
                return;
            }

            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<TweenOnPlay>>()
                         .WithAll<TweenJournal>()
                         .WithEntityAccess())
            {
                singleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                {
                    Entity = entity,
                    Event = TweenJournal.Event.OnPlay
                });
            }
            
            _tweenJournalAccess.AddDependency(ref state);
        }
    }
}
