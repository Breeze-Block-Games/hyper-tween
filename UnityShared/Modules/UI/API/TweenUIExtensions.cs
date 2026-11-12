using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.Auto.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Components;
using BreezeBlockGames.HyperTween.UnityShared.Modules.UI.Components;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using UnityEngine;
using UnityEngine.UI;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.UI.API
{
    public static class TweenUIExtensions
    {
        public static TweenHandle<T> WithColor<T>(this TweenHandle<T> tweenHandle, Image image, Color to)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.TweenBuilder.AddComponentObject(tweenHandle.Entity, image);
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, UnityObjectInstanceId.Create(image));

            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenImageColour()
            {
                Value = to
            });

            return tweenHandle;
        }
        
        public static TweenHandle<T> WithColor<T>(this TweenHandle<T> tweenHandle, Image image, Color from, Color to)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.TweenBuilder.AddComponentObject(tweenHandle.Entity, image);

            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, UnityObjectInstanceId.Create(image));
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenImageColourFrom()
            {
                Value = from
            });
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenImageColour()
            {
                Value = to
            });

            return tweenHandle;
        }
        
        public static TweenHandle<TBuilder> ImageFillTo<TBuilder>(this TweenHandle<TBuilder> tweenHandle, Image image, float to)
            where TBuilder : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle.EnsureNonNull().EnsureHasParameter();
            
            tweenHandle.TweenBuilder.AddComponentObject(tweenHandle.Entity, image);

            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, UnityObjectInstanceId.Create(image));

            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenImageFill()
            {
                Value = to
            });

            return tweenHandle;
        }
        
        public static TweenHandle<TBuilder> ImageFillTo<TBuilder>(this TweenHandle<TBuilder> tweenHandle, Image image, float from, float to)
            where TBuilder : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle.EnsureNonNull().EnsureHasParameter();
            
            tweenHandle.TweenBuilder.AddComponentObject(tweenHandle.Entity, image);
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, UnityObjectInstanceId.Create(image));

            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenImageFillFrom()
            {
                Value = from
            });
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenImageFill()
            {
                Value = to
            });

            return tweenHandle;
        }
        
        public static TweenHandle<TBuilder> CanvasGroupAlphaTo<TBuilder>(this TweenHandle<TBuilder> tweenHandle, CanvasGroup canvasGroup, float to)
            where TBuilder : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle.EnsureNonNull().EnsureHasParameter();
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, UnityObjectInstanceId.Create(canvasGroup));
            
            tweenHandle.TweenBuilder.AddComponentObject(tweenHandle.Entity, canvasGroup);

            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenCanvasGroupAlpha()
            {
                Value = to
            });

            return tweenHandle;
        }
        
        public static TweenHandle<TBuilder> CanvasGroupAlphaTo<TBuilder>(this TweenHandle<TBuilder> tweenHandle, CanvasGroup canvasGroup, float from, float to)
            where TBuilder : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle.EnsureNonNull().EnsureHasParameter();
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, UnityObjectInstanceId.Create(canvasGroup));
            
            tweenHandle.TweenBuilder.AddComponentObject(tweenHandle.Entity, canvasGroup);

            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenCanvasGroupAlphaFrom()
            {
                Value = from
            });
        
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenCanvasGroupAlpha()
            {
                Value = to
            });

            return tweenHandle;
        }
    }
}