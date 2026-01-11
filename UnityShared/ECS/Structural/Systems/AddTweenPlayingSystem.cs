using BreezeBlockGames.HyperTween.UnityShared.ECS.Sequencing.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems
{
    [UpdateInGroup(typeof(OnTweenPlaySystemGroup))]
    [BurstCompile]
    partial struct CompleteZeroDurationTweensSystem : ISystem
    {
        private EntityQuery _zeroDurationTweensQuery;
        private TweenJournalAccess _tweenJournalAccess;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CleanTweenStructuralChangeECBSystem.Singleton>();
            state.RequireForUpdate<TweenStructuralChangeECBSystem.Singleton>();

            _zeroDurationTweensQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenOnPlay>()
                // Tweens with TweenStopOnJoin won't have a duration but shouldn't be stopped
                .WithNone<TweenDuration, TweenStopOnJoin>()
            );
            
            state.RequireForUpdate(_zeroDurationTweensQuery);
            
            TweenJournalAccess.TryCreate(ref state, out _tweenJournalAccess);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var cleanEcbSingleton = SystemAPI.GetSingleton<CleanTweenStructuralChangeECBSystem.Singleton>();
            var cleanEcb = cleanEcbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            var ecbSingleton = SystemAPI.GetSingleton<TweenStructuralChangeECBSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            cleanEcb.AddComponent<TweenForceOutput>(_zeroDurationTweensQuery, EntityQueryCaptureMode.AtPlayback);
            ecb.RemoveComponent<TweenRequestPlaying>(_zeroDurationTweensQuery, EntityQueryCaptureMode.AtPlayback);
            
            if (!_tweenJournalAccess.TryGet(ref state, out var tweenJournalSingleton))
            {
                return;
            }
            
            foreach (var (_, entity) in SystemAPI.Query<RefRO<TweenOnPlay>>()
                         .WithAll<TweenJournal>()
                         .WithNone<TweenDuration, TweenStopOnJoin>().WithEntityAccess())
            {
                tweenJournalSingleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                {
                    Entity = entity,
                    Event = TweenJournal.Event.CompleteDuration
                });
            }
            
            _tweenJournalAccess.AddDependency(ref state);
        }
    }
    
    [UpdateInGroup(typeof(OnTweenPlaySystemGroup))]
    [BurstCompile]
    partial struct AddTweenPlayingSystem : ISystem
    {
        private EntityQuery _playTweensQuery;
        private TweenJournalAccess _tweenJournalAccess;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CleanTweenStructuralChangeECBSystem.Singleton>();
            state.RequireForUpdate<TweenStructuralChangeECBSystem.Singleton>();
            
            _playTweensQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenOnPlay>());

            state.RequireForUpdate(_playTweensQuery);
            
            TweenJournalAccess.TryCreate(ref state, out _tweenJournalAccess);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<TweenStructuralChangeECBSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            ecb.AddComponent<TweenPlaying>(_playTweensQuery, EntityQueryCaptureMode.AtPlayback);

            var hasJournalAccess = _tweenJournalAccess.TryGet(ref state, out var tweenJournalSingleton);
            
            foreach (var (tweenRequestPlaying, tweenDuration, tweenTimer, tweenDurationOverflow, entity) in SystemAPI
                         .Query<RefRO<TweenRequestPlaying>, RefRO<TweenDuration>, RefRW<TweenTimer>, RefRW<TweenDurationOverflow>>()
                         .WithNone<TweenPlaying>()
                         .WithEntityAccess())
            {
                tweenTimer.ValueRW.Value = tweenRequestPlaying.ValueRO.DurationOverflow;

                // Need to account for very short tweens having so much overflow that they finish immediately
                if (tweenTimer.ValueRW.Value < tweenDuration.ValueRO.Value)
                {
                    continue;
                }
                
                // If we already reached the duration, remove RequestTweenPlaying to stop the tween immediately
                tweenTimer.ValueRW.Value = tweenDuration.ValueRO.Value;
                tweenDurationOverflow.ValueRW.Value = tweenTimer.ValueRW.Value - tweenDuration.ValueRO.Value;
                
                ecb.AddComponent<TweenForceOutput>(entity);
                ecb.RemoveComponent<TweenRequestPlaying>(entity);

                if (hasJournalAccess && SystemAPI.HasComponent<TweenJournal>(entity))
                {
                    tweenJournalSingleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                    {
                        Entity = entity,
                        Event = TweenJournal.Event.UpdatedTimer,
                        Value = tweenDuration.ValueRO.Value
                    });
                    
                    tweenJournalSingleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                    {
                        Entity = entity,
                        Event = TweenJournal.Event.CompleteDuration,
                        Value = tweenDuration.ValueRO.Value
                    });
                }
            }
            
            _tweenJournalAccess.AddDependency(ref state);
        }
    }
}