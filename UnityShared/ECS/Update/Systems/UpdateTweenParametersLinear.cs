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
    [UpdateAfter(typeof(UpdateTweenTimers))]
    [BurstCompile]
    partial struct UpdateTweenParametersLinear : ISystem
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
            
            foreach (var (tweenParameter, tweenTimer, tweenDuration, entity) in SystemAPI
                         .Query<RefRW<TweenParameter>, RefRO<TweenTimer>, RefRO<TweenDuration>>()
                         .WithAny<TweenPlaying, TweenForceOutput>()
                         .WithOptions(EntityQueryOptions.FilterWriteGroup)
                         .WithEntityAccess())
            {
                tweenParameter.ValueRW.Value = tweenTimer.ValueRO.Value * tweenDuration.ValueRO.InverseValue;

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
            
            // Tweens with no duration need to have their parameter forced to 1
            foreach (var (tweenParameter, entity) in SystemAPI
                         .Query<RefRW<TweenParameter>>()
                         .WithAny<TweenPlaying, TweenForceOutput>()
                         .WithNone<TweenDuration>()
                         // No filter write group because we need to do this regardless of easing type
                         .WithEntityAccess())
            {
                tweenParameter.ValueRW.Value = 1f;
                
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