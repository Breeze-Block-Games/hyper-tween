using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Util;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Systems
{
    [BurstCompile]
    public struct AddToConflictLookupSystemJob<TMetaDataContainer> : IJobChunk where TMetaDataContainer : unmanaged
    {
        public NativeHashMap<EntityTypeKey, Entity> EntityTypeKeyToTweenMap;
        public NativeHashMap<InstanceIdKey, Entity> InstanceIdToTweenMap;
        
        public EntityCommandBuffer EntityCommandBuffer;

        public TMetaDataContainer MetaDataContainer;
        public FieldEnumerable<TMetaDataContainer, TweenComponentMetaData> TweenComponentsMetaDataEnumerable;
        
        public EntityTypeHandle EntityTypeHandle;
        
        [ReadOnly]
        public ComponentTypeHandle<TweenTarget> TweenTargetTypeHandle;

        public int IntSize;

#if HYPER_TWEEN_ENABLE_JOURNAL
        [ReadOnly]
        public ComponentTypeHandle<TweenJournal> TweenJournalTypeHandle;
        
        public TweenJournalSingleton TweenJournalSingleton;
#endif
        
        [BurstCompile]
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
            in v128 chunkEnabledMask)
        { 
            var entities = chunk.GetNativeArray(EntityTypeHandle);
            var typeInfoEnumerator = TweenComponentsMetaDataEnumerable.GetEnumerator(ref MetaDataContainer);

            while (typeInfoEnumerator.Next(out var conflictTypeTuple))
            {
                var targetComponentTypeHandle = conflictTypeTuple.TargetComponentTypeInfo.DynamicComponentTypeHandle;
                var targetComponentType = conflictTypeTuple.TargetComponentTypeInfo.ComponentType;

                if (!chunk.Has(ref targetComponentTypeHandle))
                {
                    continue;
                }

                var hasTarget = false;

                if (conflictTypeTuple.HasInstanceIdComponent)
                {
                    var instanceIdComponentTypeHandle = conflictTypeTuple.InstanceIdComponentTypeInfo.DynamicComponentTypeHandle;

                    if (chunk.Has(ref instanceIdComponentTypeHandle))
                    {
                        hasTarget = true;
                    
                        // Unity object IDs are 64-bit on newer editors; custom IDs may still be 32-bit.
                        var instanceIdSize = TypeManager.GetTypeInfo(conflictTypeTuple.InstanceIdComponentTypeInfo.ComponentType.TypeIndex).SizeInChunk;
                        var instanceIds = chunk.GetDynamicComponentDataArrayReinterpret<byte>(ref instanceIdComponentTypeHandle, instanceIdSize);
                        var instanceIds32 = instanceIdSize == sizeof(int) ? instanceIds.Reinterpret<int>(1) : default;
                        var instanceIds64 = instanceIdSize == sizeof(ulong) ? instanceIds.Reinterpret<ulong>(1) : default;
                        if (!instanceIds32.IsCreated && !instanceIds64.IsCreated)
                            throw new System.InvalidOperationException("Conflict identity components must contain a 32-bit or 64-bit ID.");

                        var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                        while (enumerator.NextEntityIndex(out var i))
                        {
                            var entity = entities[i];
                            var instanceId = instanceIds32.IsCreated ? unchecked((ulong)(uint)instanceIds32[i]) : instanceIds64[i];
                        
                            var key = new InstanceIdKey(instanceId, targetComponentType);
            
                            // No TweenTarget so we treat the Tween as the target
                            if (InstanceIdToTweenMap.TryGetValue(key, out var oldTweenEntity))
                            {
                                OnConflictDetected(entity, oldTweenEntity, chunk);

                                // This Tween is now in control of itself
                                InstanceIdToTweenMap[key] = entity;
                            }
                            else
                            {
                                // This Tween is now in control of itself
                                InstanceIdToTweenMap.Add(key, entity);
                            }
                        }
                    }
                }
                
                if (chunk.Has(ref TweenTargetTypeHandle))
                {
                    hasTarget = true;
                    
                    var tweenTargets = chunk.GetNativeArray(ref TweenTargetTypeHandle);

                    var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                    while (enumerator.NextEntityIndex(out var i))
                    {
                        var entity = entities[i];
                        var targetEntity = tweenTargets[i].Target;
                        
                        var key = new EntityTypeKey(targetEntity, targetComponentType);
                        if (EntityTypeKeyToTweenMap.TryGetValue(key, out var oldTweenEntity))
                        {
                            OnConflictDetected(entity, oldTweenEntity, chunk);

                            // This Tween is now in control of itself
                            EntityTypeKeyToTweenMap[key] = entity;
                        }
                        else
                        {
                            // This Tween is now in control of itself
                            EntityTypeKeyToTweenMap.Add(key, entity);
                        }
                    }
                }
                
                if(!hasTarget)
                {
                    var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                    while (enumerator.NextEntityIndex(out var i))
                    {
                        var entity = entities[i];
                        
                        var key = new EntityTypeKey(entity, targetComponentType);
            
                        // No TweenTarget so we treat the Tween as the target
                        if (EntityTypeKeyToTweenMap.TryGetValue(key, out var oldTweenEntity))
                        {
                            OnConflictDetected(entity, oldTweenEntity, chunk);

                            // This Tween is now in control of itself
                            EntityTypeKeyToTweenMap[key] = entity;
                        }
                        else
                        {
                            // This Tween is now in control of itself
                            EntityTypeKeyToTweenMap.Add(key, entity);
                        }
                    }
                }
            }
        }

        [BurstCompile]
        private void OnConflictDetected(Entity entity, Entity conflictedTweenEntity, ArchetypeChunk chunk)
        {
            // Stop the Tween currently in control of the target
            EntityCommandBuffer.RemoveComponent<TweenRequestPlaying>(conflictedTweenEntity);
            
            // TODO: Add TweenRequestConflict component
            EntityCommandBuffer.AddComponent<TweenConflicted>(conflictedTweenEntity);
            
#if HYPER_TWEEN_ENABLE_JOURNAL
            if (chunk.Has(ref TweenJournalTypeHandle))
            {
                TweenJournalSingleton.LiteEntries.Add(new TweenJournal.LiteEntry()
                {
                    Entity = entity,
                    Event = TweenJournal.Event.Conflict,
                    TargetEntity = conflictedTweenEntity
                });
            }
#endif
        }
    }
}