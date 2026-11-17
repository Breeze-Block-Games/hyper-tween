using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.Journal.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct TweenJournalIncrementFrameSystem : ISystem
    {
        private TweenJournalAccess _tweenJournalAccess;

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
            
            singleton.ValueRW.CurrentFrame.Value++;
            singleton.ValueRW.CurrentStructuralChangeIteration.Value = 0;
            
            _tweenJournalAccess.AddDependency(ref state);
        }
    }
}