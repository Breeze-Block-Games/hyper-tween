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
        public static BatchTweenHandle<T> WithManagedTransformOutputs<T>(this BatchTweenHandle<T> tweenHandle, NativeArray<TransformInstanceId> transformInstanceIds, int sourceIndex = 0, int destinationIndex = 0, int count = -1)
            where T : unmanaged, ITweenBuilder
        {
            count = tweenHandle.BatchTweenBuilder.GetRangeCount(transformInstanceIds.Length, sourceIndex, destinationIndex, count);
            tweenHandle.BatchTweenBuilder.AddComponentRange<LocalTransform>(destinationIndex, count);
            tweenHandle.BatchTweenBuilder.AddComponents(transformInstanceIds, sourceIndex, destinationIndex, count);
            return tweenHandle;
        }

        public static NativeArray<TransformInstanceId> ToTransformInstanceIds(this UnityEngine.Transform[] transforms, Allocator allocator)
        {
            using var profilerMarker = new ProfilerMarker("ToTransformInstanceIds").Auto();

            var transformInstanceIds = CollectionHelper.CreateNativeArray<TransformInstanceId>(
                transforms.Length, AllocatorManager.ConvertToAllocatorHandle(allocator));
            try
            {
                for (var i = 0; i < transforms.Length; i++)
                    transformInstanceIds[i] = TransformInstanceId.Create(transforms[i]);
                return transformInstanceIds;
            }
            catch
            {
                CollectionHelper.Dispose(transformInstanceIds);
                throw;
            }
        }
    }
}