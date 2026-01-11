using BreezeBlockGames.HyperTween.UnityShared.ECS.Sequencing.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Sequencing.Systems
{
    [UpdateInGroup(typeof(OnTweenPlaySystemGroup))]
    [BurstCompile]
    partial struct TweenForkOnPlaySystem : ISystem
    {
        [BurstCompile]
        public partial struct Job : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

            [BurstCompile]
            private void Execute(in DynamicBuffer<TweenForkOnPlay> forkOnPlays, in TweenRequestPlaying tweenRequestPlaying, [ChunkIndexInQuery]int chunkIndexInQuery)
            {
                foreach (var tweenForkOnPlay in forkOnPlays)
                {
                    EntityCommandBuffer.AddComponent(chunkIndexInQuery, tweenForkOnPlay.Target, tweenRequestPlaying);
                }
            }
        }
        
        [BurstCompile]
        public partial struct JobWithJournal : IJobEntity
        {
            public NativeList<TweenJournal.LiteEntry>.ParallelWriter TweenJournalWriter;

            [BurstCompile]
            private void Execute(in DynamicBuffer<TweenForkOnPlay> forkOnPlays, in Entity entity)
            {
                foreach (var tweenForkOnPlay in forkOnPlays)
                {
                    TweenJournalWriter.AddNoResize(new TweenJournal.LiteEntry()
                    {
                        Entity = entity,
                        TargetEntity = tweenForkOnPlay.Target,
                        Event = TweenJournal.Event.CompleteForkOnPlay
                    });
                }
            }
        }
        
        private EntityQuery _onPlayQuery;
        private BufferLookup<TweenForkOnPlay> _bufferLookup;
        
        private EntityQuery _onPlayWithJournalQuery;
        private TweenJournalAccess _tweenJournalAccess;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            if (TweenJournalAccess.TryCreate(ref state, out _tweenJournalAccess))
            {
                _onPlayWithJournalQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                    .WithAll<TweenForkOnPlay, TweenRequestPlaying, TweenOnPlay, TweenJournal>());
            }
            
            _onPlayQuery = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenForkOnPlay, TweenRequestPlaying, TweenOnPlay>()
            );

            _bufferLookup = state.GetBufferLookup<TweenForkOnPlay>();
            
            state.RequireForUpdate<TweenStructuralChangeECBSystem.Singleton>();
        }
        

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<TweenStructuralChangeECBSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

            _bufferLookup.Update(ref state);

            state.Dependency = new Job()
            {
                EntityCommandBuffer = ecb.AsParallelWriter(),
            }.ScheduleParallel(_onPlayQuery, state.Dependency);

            if (!_tweenJournalAccess.TryGet(ref state, out var tweenJournalSingleton))
            {
                return;
            }

            var numEntities = _onPlayWithJournalQuery.CalculateEntityCount();

            tweenJournalSingleton.ValueRW.EnsureCapacity(numEntities);

            state.Dependency = new JobWithJournal()
            {
                TweenJournalWriter = tweenJournalSingleton.ValueRW.LiteEntries.AsParallelWriter()
            }.ScheduleParallel(_onPlayWithJournalQuery, state.Dependency);
            
            _tweenJournalAccess.AddDependency(ref state);
        }
    }
}