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
                    
                    var instanceIds = chunk.GetDynamicComponentDataArrayReinterpret<int>(ref instanceIdComponentTypeHandle, IntSize);

                    var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
                    while (enumerator.NextEntityIndex(out var i))
                    {
                        var transformInstanceId = instanceIds[i];

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