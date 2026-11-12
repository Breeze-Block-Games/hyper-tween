using System;
using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.Components;
using BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms.Components;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms.API
{
    public static class TweenTransformExtensions
    {
        public static TweenHandle<TBuilder> WithTransform<TBuilder>(this TweenHandle<TBuilder> tweenHandle, UnityEngine.Transform transform) 
            where TBuilder : unmanaged, ITweenBuilder
        {
            if (!transform)
            {
                throw new NullReferenceException("Transform must be non-null");
            }

            tweenHandle = tweenHandle.EnsureHasLocalTransform();

            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, TransformInstanceId.Create(transform));
            
            return tweenHandle;
        }
    }
}