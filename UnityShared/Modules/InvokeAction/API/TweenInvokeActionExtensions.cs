using System;
using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using BreezeBlockGames.HyperTween.UnityShared.Auto.Components;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.InvokeAction.API
{
    public static class TweenInvokeActionExtensions
    {
        public static TweenHandle<TBuilder> InvokeActionOnPlay<TBuilder>(this TweenHandle<TBuilder> tweenHandle, Action<TweenInvokeActionOnPlay.Context> action) where TBuilder : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle.EnsureNonNull();
            
            tweenHandle.TweenBuilder.AddComponentObject(tweenHandle.Entity, new TweenInvokeActionOnPlay()
            {
                Action = action
            });
            
            return tweenHandle;
        }
        
        public static TweenHandle<TBuilder> InvokeActionOnStop<TBuilder>(this TweenHandle<TBuilder> tweenHandle, Action<TweenInvokeActionOnStop.Context> action) where TBuilder : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle.EnsureNonNull();
            
            tweenHandle.TweenBuilder.AddComponentObject(tweenHandle.Entity, new TweenInvokeActionOnStop()
            {
                Action = action
            });
            
            return tweenHandle;
        }
    }
}