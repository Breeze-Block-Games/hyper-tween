using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.Auto.Components;
using BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.Components;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using Unity.Mathematics;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.API
{
    public static class TweenLocalTransformExtensions
    {
        public static TweenHandle<T> WithLocalPositionOutput<T>(this TweenHandle<T> tweenHandle, float3 from, float3 to)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle
                .EnsureHasLocalTransform()
                .EnsureHasParameter();

            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenLocalPositionFrom()
            {
                Value = from
            });
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenLocalPosition()
            {
                Value = to
            });

            return tweenHandle;
        }
        
        public static TweenHandle<T> WithLocalPositionOutput<T>(this TweenHandle<T> tweenHandle, float3 to)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle
                .EnsureHasLocalTransform()
                .EnsureHasParameter();

            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenLocalPosition()
            {
                Value = to
            });

            return tweenHandle;
        }
        
        public static TweenHandle<T> WithLocalRotationOutput<T>(this TweenHandle<T> tweenHandle, quaternion from, quaternion to)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle
                .EnsureHasLocalTransform()
                .EnsureHasParameter();

            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenLocalRotationFrom()
            {
                Value = from
            });
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenLocalRotation()
            {
                Value = to
            });

            return tweenHandle;
        }
        
        public static TweenHandle<T> WithLocalRotationOutput<T>(this TweenHandle<T> tweenHandle, quaternion to)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle
                .EnsureHasLocalTransform()
                .EnsureHasParameter();

            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenLocalRotation()
            {
                Value = to
            });

            return tweenHandle;
        }
        
        public static TweenHandle<T> WithLocalUniformScaleOutput<T>(this TweenHandle<T> tweenHandle, float from, float to)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle
                .EnsureHasLocalTransform()
                .EnsureHasParameter();

            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenLocalUniformScaleFrom()
            {
                Value = from
            });
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenLocalUniformScale()
            {
                Value = to
            });

            return tweenHandle;
        }
        
        public static TweenHandle<T> WithLocalUniformScaleOutput<T>(this TweenHandle<T> tweenHandle, float to)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle
                .EnsureHasLocalTransform()
                .EnsureHasParameter();

            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenLocalUniformScale()
            {
                Value = to
            });

            return tweenHandle;
        }
    }
}