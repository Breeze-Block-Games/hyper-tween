using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Systems;
using Unity.Burst;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Systems
{
    [UpdateInGroup(typeof(UpdateTweenSystemGroup))]
    [UpdateBefore(typeof(TweenOutputSystemGroup))]
    [BurstCompile]
    partial struct UpdateTweenParametersHermiteEasing : ISystem
    {
        private TweenJournalAccess _tweenJournalAccess;
        
        public void OnCreate(ref SystemState state)
        {
            TweenJournalAccess.TryCreate(ref state, out _tweenJournalAccess);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var hasJournalAccess = _tweenJournalAccess.TryGet(ref state, out var tweenJournalSingleton);
            
            foreach (var (tweenParameter, tweenTimer, tweenDuration, tweenHermiteEasing, entity) in SystemAPI
                         .Query<RefRW<TweenParameter>, RefRO<TweenTimer>, RefRO<TweenDuration>, RefRO<TweenHermiteEasing>>()
                         .WithAny<TweenPlaying, TweenForceOutput>()
                         .WithOptions(EntityQueryOptions.FilterWriteGroup)
                         .WithEntityAccess())
            {
                var linearParameter = tweenTimer.ValueRO.Value * tweenDuration.ValueRO.InverseValue;
                
                tweenParameter.ValueRW.Value = tweenHermiteEasing.ValueRO.Interpolate(linearParameter);

                if (hasJournalAccess && SystemAPI.HasComponent<TweenJournal>(entity))
                {
                    tweenJournalSingleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                    {
                        Event = TweenJournal.Event.UpdatedParameter,
                        Entity = entity,
                        Value = tweenParameter.ValueRW.Value
                    });
                }
            }
            
            _tweenJournalAccess.AddDependency(ref state);
        }
    }
}