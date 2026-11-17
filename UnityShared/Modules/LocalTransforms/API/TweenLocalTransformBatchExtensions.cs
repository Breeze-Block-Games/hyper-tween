using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.Auto.Components;
using BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.Components;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using Unity.Collections;
using Unity.Mathematics;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.API
{
    public static class TweenLocalTransformBatchExtensions
    {
        public static BatchTweenHandle<T> WithLocalPositionOutputs<T>(this BatchTweenHandle<T> tweenHandle, NativeArray<float3> froms, NativeArray<float3> tos, int sourceIndex = 0, int destinationIndex = 0, int count = -1)
            where T : unmanaged, ITweenBuilder
        {
            count = tweenHandle.BatchTweenBuilder.GetRangeCount(froms.Length, sourceIndex, destinationIndex, count);
            tweenHandle.BatchTweenBuilder.GetRangeCount(tos.Length, sourceIndex, destinationIndex, count);
            tweenHandle.BatchTweenBuilder.AddComponentRange<TweenInitializeLocalTransform>(destinationIndex, count);
            tweenHandle.BatchTweenBuilder.AddComponents(froms.Reinterpret<TweenLocalPositionFrom>(), sourceIndex, destinationIndex, count);
            tweenHandle.BatchTweenBuilder.AddComponents(tos.Reinterpret<TweenLocalPosition>(), sourceIndex, destinationIndex, count);

            return tweenHandle;
        }

        public static BatchTweenHandle<T> WithLocalPositionOutputs<T>(this BatchTweenHandle<T> tweenHandle, NativeArray<float3> tos, int sourceIndex = 0, int destinationIndex = 0, int count = -1)
            where T : unmanaged, ITweenBuilder
        {
            count = tweenHandle.BatchTweenBuilder.GetRangeCount(tos.Length, sourceIndex, destinationIndex, count);
            tweenHandle.BatchTweenBuilder.AddComponentRange<TweenInitializeLocalTransform>(destinationIndex, count);
            tweenHandle.BatchTweenBuilder.AddComponents(tos.Reinterpret<TweenLocalPosition>(), sourceIndex, destinationIndex, count);

            return tweenHandle;
        }

        public static BatchTweenHandle<T> WithLocalRotationOutputs<T>(this BatchTweenHandle<T> tweenHandle, NativeArray<quaternion> froms, NativeArray<quaternion> tos, int sourceIndex = 0, int destinationIndex = 0, int count = -1)
            where T : unmanaged, ITweenBuilder
        {
            count = tweenHandle.BatchTweenBuilder.GetRangeCount(froms.Length, sourceIndex, destinationIndex, count);
            tweenHandle.BatchTweenBuilder.GetRangeCount(tos.Length, sourceIndex, destinationIndex, count);
            tweenHandle.BatchTweenBuilder.AddComponentRange<TweenInitializeLocalTransform>(destinationIndex, count);
            tweenHandle.BatchTweenBuilder.AddComponents(froms.Reinterpret<TweenLocalRotationFrom>(), sourceIndex, destinationIndex, count);
            tweenHandle.BatchTweenBuilder.AddComponents(tos.Reinterpret<TweenLocalRotation>(), sourceIndex, destinationIndex, count);

            return tweenHandle;
        }

        public static BatchTweenHandle<T> WithLocalRotationOutputs<T>(this BatchTweenHandle<T> tweenHandle, NativeArray<quaternion> tos, int sourceIndex = 0, int destinationIndex = 0, int count = -1)
            where T : unmanaged, ITweenBuilder
        {
            count = tweenHandle.BatchTweenBuilder.GetRangeCount(tos.Length, sourceIndex, destinationIndex, count);
            tweenHandle.BatchTweenBuilder.AddComponentRange<TweenInitializeLocalTransform>(destinationIndex, count);
            tweenHandle.BatchTweenBuilder.AddComponents(tos.Reinterpret<TweenLocalRotation>(), sourceIndex, destinationIndex, count);

            return tweenHandle;
        }

        public static BatchTweenHandle<T> WithLocalUniformScaleOutputs<T>(this BatchTweenHandle<T> tweenHandle, NativeArray<float> froms, NativeArray<float> tos, int sourceIndex = 0, int destinationIndex = 0, int count = -1)
            where T : unmanaged, ITweenBuilder
        {
            count = tweenHandle.BatchTweenBuilder.GetRangeCount(froms.Length, sourceIndex, destinationIndex, count);
            tweenHandle.BatchTweenBuilder.GetRangeCount(tos.Length, sourceIndex, destinationIndex, count);
            tweenHandle.BatchTweenBuilder.AddComponentRange<TweenInitializeLocalTransform>(destinationIndex, count);
            tweenHandle.BatchTweenBuilder.AddComponents(froms.Reinterpret<TweenLocalUniformScaleFrom>(), sourceIndex, destinationIndex, count);
            tweenHandle.BatchTweenBuilder.AddComponents(tos.Reinterpret<TweenLocalUniformScale>(), sourceIndex, destinationIndex, count);

            return tweenHandle;
        }

        public static BatchTweenHandle<T> WithLocalUniformScaleOutputs<T>(this BatchTweenHandle<T> tweenHandle, NativeArray<float> tos, int sourceIndex = 0, int destinationIndex = 0, int count = -1)
            where T : unmanaged, ITweenBuilder
        {
            count = tweenHandle.BatchTweenBuilder.GetRangeCount(tos.Length, sourceIndex, destinationIndex, count);
            tweenHandle.BatchTweenBuilder.AddComponentRange<TweenInitializeLocalTransform>(destinationIndex, count);
            tweenHandle.BatchTweenBuilder.AddComponents(tos.Reinterpret<TweenLocalUniformScale>(), sourceIndex, destinationIndex, count);

            return tweenHandle;
        }
    }
}