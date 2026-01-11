#if UNITY_INCLUDE_TESTS && LIT_MOTION_PERFORMANCE_TESTS
using System;
using LitMotion;
using LitMotion.Adapters;
using LitMotion.Extensions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BreezeBlockGames.HyperTween.UnityShared.Tests
{
    public class LitMotionPerformanceTests : BasePerformanceTests
    {
        private readonly CompositeMotionHandle _compositeMotionHandle = new();
        
        protected override void CreateTransformTweens(World world, Transform[] transforms, NativeArray<float3> positions, float duration)
        {
            MotionDispatcher.EnsureStorageCapacity<Vector3, NoOptions, Vector3MotionAdapter>(transforms.Length + 1);
            for (var i = 0; i < transforms.Length; i++)
            {
                LMotion.Create(Vector3.zero, positions[i], duration)
                    .BindToPosition(transforms[i])
                    .AddTo(_compositeMotionHandle);
            }
        }

        protected override void CreateDirectLocalTransformTweens(World world, NativeArray<Entity> entities, NativeArray<float3> positions, float duration)
        {
            throw new NotImplementedException();
        }

        protected override void CreateIndirectLocalTransformTweens(World world, NativeArray<Entity> entities, NativeArray<float3> positions, float duration)
        {
            throw new NotImplementedException();
        }

         protected override string[] GetUpdateProfileMarkers()
        {
            return new string[] 
            { 
                "LitMotionUpdate"
            };
        }

        protected override void Dispose()
        {
            _compositeMotionHandle.Cancel();
        }

        protected override World CreateWorld()
        {
            return null!;
        }
    }
}
#endif