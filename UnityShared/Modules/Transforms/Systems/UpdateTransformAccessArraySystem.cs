using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Systems;
using BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.Components;
using BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms.Systems
{
    [UpdateInGroup(typeof(UpdateTweenSystemGroup))]
    [UpdateBefore(typeof(TweenTransformStructuralChangeECBSystem))]
    [BurstCompile]
    public partial struct UpdateTransformAccessArraySystem : ISystem
    {
        private ComponentLookup<TransformInstanceId> _transformInstanceIdLookup;

        private EntityQuery _removalQuery, _additionQuery, _missingLocalToWorldQuery;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TweenTransformStructuralChangeECBSystem.Singleton>();
            state.EntityManager.CreateSingleton(new TransformAccessSingleton()
            {
                TransformAccessArray = new TransformAccessArray(64),
                EntityLookup = new NativeList<Entity>(64, Allocator.Persistent),
                IndexLookup = new NativeHashMap<Entity, int>(64, Allocator.Persistent),
            });
            state.RequireForUpdate<TransformAccessSingleton>();
            
            _transformInstanceIdLookup = state.GetComponentLookup<TransformInstanceId>();

            _removalQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<InTransformAccessArray>()
                .WithNone<TweenPlaying, TweenForceOutput>()
                .Build(ref state);

            _additionQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TransformInstanceId>()
                .WithAny<TweenPlaying, TweenForceOutput>()
                .WithNone<InTransformAccessArray, InvalidTransformInstance>()
                .Build(ref state);
            
            _missingLocalToWorldQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TransformInstanceId>()
                .WithAny<TweenPlaying, TweenForceOutput>()
                .WithNone<LocalToWorld, InvalidTransformInstance>()
                .Build(ref state);
        }

        public void OnDestroy(ref SystemState state)
        {
            ref var conflictLookup = ref SystemAPI.GetSingletonRW<TransformAccessSingleton>().ValueRW;
            conflictLookup.Dispose();        
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var singleton = SystemAPI.GetSingleton<TransformAccessSingleton>();
            
            var ecbSingleton = SystemAPI.GetSingleton<TweenTransformStructuralChangeECBSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            
            _transformInstanceIdLookup.Update(ref state);
            
            var requiredCapacity = math.ceilpow2(singleton.EntityLookup.Length - _removalQuery.CalculateEntityCount() + _additionQuery.CalculateEntityCount());
            if (singleton.EntityLookup.Capacity < requiredCapacity)
            {
                singleton.EntityLookup.Capacity = requiredCapacity;
                singleton.TransformAccessArray.capacity = requiredCapacity;
            }
            
            foreach (var (_, entity) in SystemAPI
                         .Query<RefRO<InTransformAccessArray>>()
                         .WithNone<TweenPlaying, TweenForceOutput>()
                         .WithEntityAccess())
            {
                var removeIndex = singleton.IndexLookup[entity];
                var backEntity = singleton.EntityLookup[^1];

                singleton.IndexLookup[backEntity] = removeIndex;
                singleton.EntityLookup.RemoveAtSwapBack(removeIndex);
                singleton.TransformAccessArray.RemoveAtSwapBack(removeIndex);
            }
            
            ecb.RemoveComponent<InTransformAccessArray>(_removalQuery, EntityQueryCaptureMode.AtPlayback);
            
            ecb.AddComponent<InTransformAccessArray>(_additionQuery, EntityQueryCaptureMode.AtPlayback);

            foreach (var (transformInstanceId, localTransform, entity) in SystemAPI
                         .Query<RefRO<TransformInstanceId>, RefRW<LocalTransform>>()
                         .WithAny<TweenPlaying, TweenForceOutput>()
                         .WithNone<InTransformAccessArray, InvalidTransformInstance>()
                         .WithEntityAccess())
            {
                var length = singleton.TransformAccessArray.length;

                singleton.TransformAccessArray.Add(transformInstanceId.ValueRO.Value);

                var transformHandle = singleton.TransformAccessArray.GetTransformHandle(length);
                
                localTransform.ValueRW.Position = transformHandle.localPosition;
                localTransform.ValueRW.Rotation = transformHandle.localRotation;
                localTransform.ValueRW.Scale = transformHandle.localScale.x;
                
                // Yep, if the transform is invalid, Unity just doesn't add to the array, no errors or anything...
                if (length == singleton.TransformAccessArray.length)
                {
                    Debug.LogError($"Failed to add to TransformAccessArray: {entity}");
                    ecb.RemoveComponent<InTransformAccessArray>(entity);
                    ecb.AddComponent<InvalidTransformInstance>(entity);
                    continue;
                }
                
                singleton.IndexLookup[entity] = length;
                singleton.EntityLookup.Add(entity);
            }

            ecb.AddComponent<LocalToWorld>(_missingLocalToWorldQuery, EntityQueryCaptureMode.AtPlayback);
        }
    }
}