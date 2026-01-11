using Unity.Burst;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.Journal.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct TweenJournalClearSystem : ISystem
    {
        private TweenJournalAccess _tweenJournalAccess;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.Enabled = TweenJournalAccess.TryCreate(ref state, out _tweenJournalAccess);
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!_tweenJournalAccess.TryGet(ref state, out var singleton))
            {
                return;
            }

            if (singleton.ValueRW.Consumed.Value)
            {
                singleton.ValueRW.Entries.Clear();
                singleton.ValueRW.NameLookup.Clear();
                singleton.ValueRW.Consumed.Value = false;
            }
            
            _tweenJournalAccess.AddDependency(ref state);
        }
    }
}