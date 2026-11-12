using BreezeBlockGames.HyperTween.UnityShared.ECS.Sequencing.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Systems;
using Unity.Burst;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Sequencing.Systems
{
    [UpdateInGroup(typeof(OnTweenStopSystemGroup))]
    [BurstCompile]
    partial struct TweenPlayOnStopSystem : ISystem
    {
        private TweenJournalAccess _tweenJournalAccess;

        [BurstCompile]
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

            var hasJournalAccess = _tweenJournalAccess.TryGet(ref state, out var tweenJournalSingleton);
            
            foreach (var (tweenPlayOnStop, tweenDurationOverflow, entity) in SystemAPI
                         .Query<RefRO<TweenPlayOnStop>, RefRO<TweenDurationOverflow>>()
                         .WithAll<TweenOnStop>()
                         .WithEntityAccess())
            {
                ecb.AddComponent(tweenPlayOnStop.ValueRO.Target, new TweenRequestPlaying()
                {
                    DurationOverflow = tweenDurationOverflow.ValueRO.Value
                });

                if (hasJournalAccess && SystemAPI.HasComponent<TweenJournal>(entity))
                {
                    tweenJournalSingleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                    {
                        Entity = entity,
                        TargetEntity = tweenPlayOnStop.ValueRO.Target,
                        Event = TweenJournal.Event.CompletePlayOnStop
                    });
                }
            }
            
            _tweenJournalAccess.AddDependency(ref state);
        }
    }
}