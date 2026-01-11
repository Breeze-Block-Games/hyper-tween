using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace BreezeBlockGames.HyperTween.UnityShared.Journal.Systems
{
    [UpdateInGroup(typeof(TweenStructuralChangeSystemGroup))]
    [UpdateBefore(typeof(CleanTweenStructuralChangeECBSystem))]
    [UpdateAfter(typeof(OnTweenPlaySystemGroup))]
    [BurstCompile]
    public partial struct TweenJournalCommitSystem : ISystem
    {
        [BurstCompile]
        private struct CommitJob : IJob
        {
            public TweenJournalSingleton Singleton;
            public double Time;
            
            [BurstCompile]
            public void Execute()
            {
                var index = Singleton.Index.Value;
                var length = Singleton.Length.Value;
                var count = Singleton.Count.Value;
                var frame = Singleton.CurrentFrame.Value;
                var iteration = Singleton.CurrentStructuralChangeIteration.Value;
            
                foreach (var entry in Singleton.LiteEntries)
                {

                    Singleton.Entries.Add(new TweenJournal.Entry(entry, frame, Time, count, iteration));
                    count++;
                }
                Singleton.LiteEntries.Clear();
                Singleton.Index.Value = index;
                Singleton.Length.Value = length;
                Singleton.Count.Value = count;
            }
        }
        
        private TweenJournalAccess _tweenJournalAccess;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.Enabled = TweenJournalAccess.TryCreate(ref state, out _tweenJournalAccess);
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();
            
            if (!_tweenJournalAccess.TryGet(ref state, out var singleton))
            {
                return;
            }

            if (singleton.ValueRO.LiteEntries.Length == 0)
            {
                return;
            }

            foreach (var liteEntry in singleton.ValueRW.LiteEntries)
            {
                if (!singleton.ValueRW.NameLookup.ContainsKey(liteEntry.Entity))
                {
                    state.EntityManager.GetName(liteEntry.Entity, out var name);
                    singleton.ValueRW.NameLookup.Add(liteEntry.Entity, name);
                }

                if (liteEntry.TargetEntity.Equals(Entity.Null))
                {
                    continue;
                }
                
                if (!singleton.ValueRW.NameLookup.ContainsKey(liteEntry.TargetEntity))
                {
                    state.EntityManager.GetName(liteEntry.TargetEntity, out var name);
                    singleton.ValueRW.NameLookup.Add(liteEntry.TargetEntity, name);
                }
            }
            singleton.ValueRW.CurrentStructuralChangeIteration.Value++;
            
            state.Dependency = new CommitJob()
                {
                    Singleton = singleton.ValueRW,
                    Time = SystemAPI.Time.ElapsedTime
                }
                .Schedule(state.Dependency);
            
            _tweenJournalAccess.AddDependency(ref state);
        }
    }
}