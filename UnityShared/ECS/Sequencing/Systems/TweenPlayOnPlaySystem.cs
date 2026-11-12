using BreezeBlockGames.HyperTween.UnityShared.ECS.Sequencing.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Systems;
using Unity.Burst;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Sequencing.Systems
{
    [UpdateInGroup(typeof(OnTweenPlaySystemGroup))]
    [BurstCompile]
    partial struct TweenPlayOnPlaySystem : ISystem
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

            var hasJournalAccess = _tweenJournalAccess.TryGet(ref state, out var tweenJournalSingleton);

            foreach (var (tweenPlayOnPlay, tweenRequestPlaying, entity) in SystemAPI
                         .Query<RefRO<TweenPlayOnPlay>, RefRO<TweenRequestPlaying>>()
                         .WithAll<TweenOnPlay>()
                         .WithEntityAccess())
            {
                ecb.AddComponent(tweenPlayOnPlay.ValueRO.Target, new TweenRequestPlaying()
                {
                    DurationOverflow = tweenRequestPlaying.ValueRO.DurationOverflow
                });

                if (hasJournalAccess && SystemAPI.HasComponent<TweenJournal>(entity))
                {
                    tweenJournalSingleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                    {
                        Entity = entity,
                        TargetEntity = tweenPlayOnPlay.ValueRO.Target,
                        Event = TweenJournal.Event.CompletePlayOnPlay
                    });
                    
                    ecb.AddComponent<TweenJournal>(tweenPlayOnPlay.ValueRO.Target);
                }
            }
            
            _tweenJournalAccess.AddDependency(ref state);
        }
    }
}