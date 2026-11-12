using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Components;
using BreezeBlockGames.HyperTween.UnityShared.Modules.R3.Components;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using R3;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.R3.API
{
    public static class TweenReactivePropertyExtensions
    {
        public static TweenHandle<TBuilder> IntReactivePropertyTo<TBuilder>(this TweenHandle<TBuilder> tweenHandle, ReactiveProperty<int> reactiveProperty, int to)
            where TBuilder : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle.EnsureNonNull().EnsureHasParameter();
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, ObjectHashCode.Create(reactiveProperty));
            
            tweenHandle.TweenBuilder.AddComponentObject(tweenHandle.Entity, new IntReactivePropertyComponent()
            {
                Value = reactiveProperty
            });

            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenIntReactiveProperty()
            {
                Value = to
            });

            return tweenHandle;
        }
    }
}