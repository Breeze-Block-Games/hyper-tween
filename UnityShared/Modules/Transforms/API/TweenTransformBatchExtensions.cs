using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.Components;
using BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms.Components;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using Unity.Collections;
using Unity.Profiling;
using Unity.Transforms;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms.API
{
    public static class TweenTransformBatchExtensions
    {
        public static BatchTweenHandle<T> WithManagedTransformOutputs<T>(this BatchTweenHandle<T> tweenHandle, NativeArray<TransformInstanceId> transformInstanceIds)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.BatchTweenBuilder.AddComponent<LocalTransform>();
            tweenHandle.BatchTweenBuilder.AddComponents(transformInstanceIds);
            return tweenHandle;
        }
        
        public static NativeArray<TransformInstanceId> ToTransformInstanceIds(this UnityEngine.Transform[] transforms, Allocator allocator)
        {
            using var profilerMarker = new ProfilerMarker("ToTransformInstanceIds").Auto();
            
            var transformInstanceIds = new NativeArray<TransformInstanceId>(transforms.Length, allocator);
            
            for (var i = 0; i < transforms.Length; i++)
            {
                transformInstanceIds[i] = TransformInstanceId.Create(transforms[i]);
            }

            return transformInstanceIds;
        }
    }
}