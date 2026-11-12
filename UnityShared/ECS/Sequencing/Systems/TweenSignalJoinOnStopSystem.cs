using BreezeBlockGames.HyperTween.UnityShared.ECS.Sequencing.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Sequencing.Systems
{
    [UpdateInGroup(typeof(OnTweenStopSystemGroup))]
    [BurstCompile]
    partial struct TweenSignalJoinOnStopSystem : ISystem
    {
        private struct GatherData
        {
            public int Count;
            public float MaxDurationOverflow;
        }

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

            NativeHashMap<Entity, GatherData> gatherMap = new NativeHashMap<Entity, GatherData>(16, Allocator.Temp);

            try
            {
                var hasTweenJournalAccess = _tweenJournalAccess.TryGet(ref state, out var tweenJournalSingleton);
                
                foreach (var (tweenIncrementGatherOnStop, tweenDurationOverflow, entity) in SystemAPI
                             .Query<RefRO<TweenSignalJoinOnStop>, RefRO<TweenDurationOverflow>>()
                             .WithAll<TweenOnStop>()
                             .WithEntityAccess())
                {
                    var tweenEntity = tweenIncrementGatherOnStop.ValueRO.Target;

                    if (hasTweenJournalAccess && SystemAPI.HasComponent<TweenJournal>(entity))
                    {
                        tweenJournalSingleton.ValueRW.LiteEntries.Add(new TweenJournal.LiteEntry()
                        {
                            Entity = entity,
                            TargetEntity = tweenEntity,
                            Event = TweenJournal.Event.CompleteSignalJoinOnStop
                        });
                    }
                    
                    if (gatherMap.TryGetValue(tweenEntity, out var gatherData))
                    {
                        gatherData.Count++;
                        gatherData.MaxDurationOverflow = math.max(gatherData.MaxDurationOverflow, tweenDurationOverflow.ValueRO.Value);
                        
                        gatherMap[tweenEntity] = gatherData;
                    }
                    else
                    {
                        gatherMap.Add(tweenEntity, new GatherData()
                        {
                            Count = 1,
                            MaxDurationOverflow = tweenDurationOverflow.ValueRO.Value
                        });
                    }
                }
                
                foreach (var pair in gatherMap)
                {
                    var existingGather = SystemAPI.GetComponent<TweenStopOnJoin>(pair.Key);
                    existingGather.CurrentSignals += pair.Value.Count;
                    
                    ecb.SetComponent(pair.Key, existingGather);

                    if (existingGather.CurrentSignals >= existingGather.RequiredSignals)
                    {
                        // We only care about the duration overflow in this frame
                        ecb.SetComponent(pair.Key, new TweenDurationOverflow()
                        {
                            Value = pair.Value.MaxDurationOverflow
                        });
                        
                        ecb.RemoveComponent<TweenRequestPlaying>(pair.Key);
                    }
                    
                    if (existingGather.CurrentSignals > existingGather.RequiredSignals)
                    {
                        Debug.LogError($"Gather exceeded threshold: {pair.Key}");
                    }
                }
                
                _tweenJournalAccess.AddDependency(ref state);
            }
            finally
            {
                gatherMap.Dispose();
            }
        }
    }
}