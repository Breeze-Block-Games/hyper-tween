using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Systems;
using Unity.Burst;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Systems
{
    [UpdateInGroup(typeof(UpdateTweenSystemGroup))]
    [BurstCompile]
    partial struct UpdateTweenTimers : ISystem
    {
        private TweenJournalAccess _tweenJournalAccess;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TweenStructuralChangeECBSystem.Singleton>();
            
            TweenJournalAccess.TryCreate(ref state, out _tweenJournalAccess);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<TweenStructuralChangeECBSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
            var dt = SystemAPI.Time.DeltaTime;

            var hasJournalAccess = _tweenJournalAccess.TryGet(ref state, out var tweenJournalSingleton);
            
            foreach (var (tweenTimer, tweenDuration, tweenDurationOverflow, entity) in SystemAPI
                         .Query<RefRW<TweenTimer>, RefRO<TweenDuration>, RefRW<TweenDurationOverflow>>()
                         .WithAll<TweenPlaying>()
                         .WithEntityAccess())
            {
                tweenTimer.ValueRW.Value += dt;

                var hasJournal = hasJournalAccess && SystemAPI.HasComponent<TweenJournal>(entity);
                
                if (tweenTimer.ValueRO.Value < tweenDuration.ValueRO.Value)
                {
                    if (hasJournal)
                    {
                        tweenJournalSingleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                        {
                            Entity = entity,
                            Event = TweenJournal.Event.UpdatedTimer,
                            Value = tweenTimer.ValueRW.Value
                        });
                    }
                    
                    continue;
                }

                tweenDurationOverflow.ValueRW.Value = tweenTimer.ValueRW.Value - tweenDuration.ValueRO.Value;
                tweenTimer.ValueRW.Value = tweenDuration.ValueRO.Value;

                ecb.RemoveComponent<TweenRequestPlaying>(entity);

                if (hasJournal)
                {
                    tweenJournalSingleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                    {
                        Entity = entity,
                        Event = TweenJournal.Event.UpdatedTimer,
                        Value = tweenTimer.ValueRW.Value
                    });
                    
                    tweenJournalSingleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                    {
                        Entity = entity,
                        Event = TweenJournal.Event.CompleteDuration
                    });
                }
            }
            
            _tweenJournalAccess.AddDependency(ref state);
        }
    }
}
