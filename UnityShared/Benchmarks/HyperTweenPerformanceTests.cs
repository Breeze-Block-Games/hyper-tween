#if UNITY_INCLUDE_TESTS
using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.Components;
using BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms.API;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace BreezeBlockGames.HyperTween.UnityShared.Tests
{
    [BurstCompile]
    public class HyperTweenPerformanceTests : BasePerformanceTests
    {
        [BurstCompile]
        private struct CreateDirectTweensJob : IJobParallelForBatch
        {
            public TweenFactory<EntityCommandBufferParallelWriterTweenBuilder> TweenFactory;
            
            public float Duration;
            
            [ReadOnly]
            public NativeArray<Entity> Targets;

            [ReadOnly]
            public NativeArray<float3> Positions;
            
            [BurstCompile]
            public void Execute(int startIndex, int count)
            {
                using var batchTweenHandle = TweenFactory
                    .CreateBatch(Targets.GetSubArray(startIndex, count))
                    .WithDuration(Duration)
                    .WithLocalPositionOutputs(Positions.GetSubArray(startIndex, count))
                    .Play();
            }
        }
        
        [BurstCompile]
        private struct CreateIndirectTweensJob : IJobParallelForBatch
        {
            public TweenFactory<EntityCommandBufferParallelWriterTweenBuilder> TweenFactory;
            
            public float Duration;
            
            [ReadOnly]
            public NativeArray<Entity> Targets;

            [ReadOnly]
            public NativeArray<float3> Positions;
            
            [BurstCompile]
            public void Execute(int startIndex, int count)
            {
                using var batchTweenHandle = TweenFactory.CreateTween()
                    .WithDuration(Duration)
                    .Play()
                    .CreateBatch(count, Allocator.Temp)
                    .WithTargets(Targets.GetSubArray(startIndex, count))
                    .WithLocalPositionOutputs(Positions.GetSubArray(startIndex, count));
            }
        }
        
        [BurstCompile]
        private struct CreateManagedTransformTweensJob : IJob, IJobParallelForBatch
        {
            public TweenFactory<EntityCommandBufferParallelWriterTweenBuilder> TweenFactory;
            
            public float Duration;
            
            [ReadOnly]
            public NativeArray<TransformInstanceId> TransformInstanceIds;

            [ReadOnly]
            public NativeArray<float3> Positions;
            
            [BurstCompile]
            public void Execute(int startIndex, int count)
            {
                using var batchTweenHandle = TweenFactory.CreateTween()
                    .WithDuration(Duration)
                    .Play()
                    .CreateBatch(count, Allocator.Temp)
                    .WithManagedTransformOutputs(TransformInstanceIds.GetSubArray(startIndex, count))
                    .WithLocalPositionOutputs(Positions.GetSubArray(startIndex, count));            
            }

            [BurstCompile]
            public void Execute()
            {
                using var batchTweenHandle = TweenFactory.CreateTween()
                    .WithDuration(Duration)
                    .Play()
                    .CreateBatch(TransformInstanceIds.Length, Allocator.Temp)
                    .WithManagedTransformOutputs(TransformInstanceIds)
                    .WithLocalPositionOutputs(Positions);    
            }
        }
        
        protected override World CreateWorld()
        {
            var world = DefaultWorldInitialization.Initialize("PerformanceTests");
            
            // Disable these systems, we don't need them, and they're skewing the Create tests
            world.Unmanaged.GetExistingSystemState<CompanionGameObjectUpdateTransformSystem>().Enabled = false;
            
            // One update to get systems to initialize and avoid noise in the actual tests
            world.Update();
            
            return world;
        }

        protected override void CreateTransformTweens(World world, Transform[] transforms, NativeArray<float3> positions, float duration)
        {
            using (new ProfilerMarker("CreateTween").Auto())
            {
                var exclusiveEntityTransactionScope = new ExclusiveEntityTransactionScope(world);
                
                var tweenFactory = world
                    .CreateTweenFactory()
                    .AsParallelWriter();

                using var transformInstanceIds = transforms.ToTransformInstanceIds(Allocator.TempJob);
                
                new CreateManagedTransformTweensJob()
                { 
                    TweenFactory = tweenFactory,
                    Duration = duration,
                    TransformInstanceIds = transformInstanceIds,
                    Positions = positions 
                }.ScheduleBatch(positions.Length, 1024).Complete();
                
                exclusiveEntityTransactionScope.Playback();
            }

            using (new ProfilerMarker("World.Update").Auto())
            {
                world.Update();
            }
        }

        protected override void CreateDirectLocalTransformTweens(World world, NativeArray<Entity> entities, NativeArray<float3> positions, float duration)
        {
            using (new ProfilerMarker("CreateTween").Auto())
            {
                var tweenFactory = world
                    .CreateTweenFactory()
                    .AsParallelWriter();

                new CreateDirectTweensJob()
                { 
                    TweenFactory = tweenFactory,
                    Duration = duration,
                    Targets = entities,
                    Positions = positions 
                }.ScheduleBatch(entities.Length, 1024).Complete();
            }

            using (new ProfilerMarker("World.Update").Auto())
            {
                world.Update();
            }
        }

        protected override void CreateIndirectLocalTransformTweens(World world, NativeArray<Entity> entities, NativeArray<float3> positions, float duration)
        {
            using (new ProfilerMarker("CreateTween").Auto())
            {
                var tweenFactory = world
                    .CreateTweenFactory()
                    .AsParallelWriter();

                new CreateIndirectTweensJob()
                { 
                    TweenFactory = tweenFactory,
                    Duration = duration,
                    Targets = entities,
                    Positions = positions 
                }.ScheduleBatch(entities.Length, 1024).Complete();
            }

            using (new ProfilerMarker("World.Update").Auto())
            {
                world.Update();
            }
        }
        
        protected override string[] GetUpdateProfileMarkers()
        {
            return new string[] 
            { 
                "PerformanceTests Unity.Entities.SimulationSystemGroup"
            };
        }

        protected override void Dispose()
        {
        }
    }
}
#endif