using System;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Util;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Systems
{
    
    [BurstCompile]
    public struct RemoveFromConflictLookupJob<TMetaDataContainer> : IJobChunk where TMetaDataContainer : unmanaged
    {
        public NativeHashMap<EntityTypeKey, Entity> TargetToTweenMap;
        public NativeHashMap<InstanceIdKey, Entity> GameObjectTypeKeyToTweenMap;

        public TMetaDataContainer MetaDataContainer;
        public FieldEnumerable<TMetaDataContainer, TweenComponentMetaData> TweenComponentsMetaDataEnumerable;
        
        public EntityTypeHandle EntityTypeHandle;
        
        [ReadOnly]
        public ComponentTypeHandle<TweenTarget> TweenTargetTypeHandle;

        public int IntSize;
        
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

                var instanceIdComponentTypeHandle = conflictTypeTuple.InstanceIdComponentTypeInfo.DynamicComponentTypeHandle;
                
                var hasTarget = false;
                
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
                        var transformInstanceId = instanceIds32.IsCreated ? unchecked((ulong)(uint)instanceIds32[i]) : instanceIds64[i];

                        if (!GameObjectTypeKeyToTweenMap.Remove(new InstanceIdKey(transformInstanceId, targetComponentType)))
                        {
                            throw new InvalidOperationException("Tween Entity does not exist in ConflictLookup");
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
                        var entity = tweenTargets[i].Target;

                        if (!TargetToTweenMap.Remove(new EntityTypeKey(entity, targetComponentType)))
                        {
                            throw new InvalidOperationException("Tween Entity does not exist in ConflictLookup");
                        }
                    }
                }
                
                if(!hasTarget)
                {
                    var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                    while (enumerator.NextEntityIndex(out var i))
                    {
                        var entity = entities[i];
                        if (!TargetToTweenMap.Remove(new EntityTypeKey(entity, targetComponentType)))
                        {
                            throw new InvalidOperationException("Tween Entity does not exist in ConflictLookup");
                        }
                    }
                }
            }
        }
    }
        
    
}