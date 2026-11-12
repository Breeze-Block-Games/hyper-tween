using BreezeBlockGames.HyperTween.UnityShared.ECS.Sequencing.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.Journal.Systems
{
    [UpdateInGroup(typeof(TweenStructuralChangeSystemGroup))]
    [UpdateBefore(typeof(OnTweenStopSystemGroup))]
    public partial struct TweenJournalCreateSystem : ISystem
    {
        private EntityQuery _tweenJournal;
        private TweenJournalAccess _tweenJournalAccess;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            TweenJournalSingleton.Create(ref state);

            state.RequireForUpdate<CleanTweenStructuralChangeECBSystem.Singleton>();
            state.RequireForUpdate<TweenJournalSingleton>();
            state.RequireForUpdate<TweenJournal>();

            _tweenJournal = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenJournal>()
                .WithNone<TweenInJournal>());
            
            state.Enabled = TweenJournalAccess.TryCreate(ref state, out _tweenJournalAccess);
        }
        
        public void OnDestroy(ref SystemState state)
        {
            var singleton = SystemAPI.GetSingleton<TweenJournalSingleton>();
            singleton.Dispose();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!_tweenJournalAccess.TryGet(ref state, out var singleton))
            {
                return;
            }
            
            var cleanEcbSingleton = SystemAPI.GetSingleton<CleanTweenStructuralChangeECBSystem.Singleton>();
            var cleanEcb = cleanEcbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (_, entity) in SystemAPI.Query<RefRO<TweenJournal>>()
                         .WithNone<TweenInJournal>()
                         .WithEntityAccess())
            {
                singleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                {
                    Entity = entity,
                    Event = TweenJournal.Event.Create
                });
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<TweenDuration>>()
                         .WithAll<TweenJournal>()
                         .WithNone<TweenInJournal>()
                         .WithEntityAccess())
            {
                singleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                {
                    Entity = entity,
                    Event = TweenJournal.Event.CreateDuration
                });
            }
            
            foreach (var (forkOnPlays, entity) in SystemAPI.Query<DynamicBuffer<TweenForkOnPlay>>()
                         .WithAll<TweenJournal>()
                         .WithNone<TweenInJournal>()
                         .WithEntityAccess())
            {
                foreach (var forkOnPlay in forkOnPlays)
                {
                    singleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                    {
                        Entity = entity,
                        Event = TweenJournal.Event.CreateForkOnPlay,
                        TargetEntity = forkOnPlay.Target
                    });
                }
            }
            
            foreach (var (playOnPlay, entity) in SystemAPI.Query<RefRO<TweenPlayOnPlay>>()
                         .WithAll<TweenJournal>()
                         .WithNone<TweenInJournal>()
                         .WithEntityAccess())
            {
                singleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                {
                    Entity = entity,
                    Event = TweenJournal.Event.CreatePlayOnPlay,
                    TargetEntity = playOnPlay.ValueRO.Target
                });
            }
            
            foreach (var (playOnStop, entity) in SystemAPI.Query<RefRO<TweenPlayOnStop>>()
                         .WithAll<TweenJournal>()
                         .WithNone<TweenInJournal>()
                         .WithEntityAccess())
            {
                singleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                {
                    Entity = entity,
                    Event = TweenJournal.Event.CreatePlayOnStop,
                    TargetEntity = playOnStop.ValueRO.Target
                });
            }
            
            foreach (var (signalOnStop, entity) in SystemAPI.Query<RefRO<TweenSignalJoinOnStop>>()
                         .WithAll<TweenJournal>()
                         .WithNone<TweenInJournal>()
                         .WithEntityAccess())
            {
                singleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                {
                    Entity = entity,
                    Event = TweenJournal.Event.CreateSignalJoinOnStop,
                    TargetEntity = signalOnStop.ValueRO.Target
                });
            }

            cleanEcb.AddComponent<TweenInJournal>(_tweenJournal, EntityQueryCaptureMode.AtPlayback);
            _tweenJournalAccess.AddDependency(ref state);
        }
    }
}