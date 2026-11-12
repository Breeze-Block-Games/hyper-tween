using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.Auto.Components;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using UnityEngine.Playables;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.Timeline.API
{
    public static class TweenPlayableDirectorExtensions
    {
        public static TweenHandle<T> WithPlayableDirector<T>(this TweenHandle<T> tweenHandle, PlayableDirector playableDirector)
            where T : unmanaged, ITweenBuilder
        {
            return tweenHandle.WithPlayableDirector(playableDirector, playableDirector.playableAsset);
        }
        
        public static TweenHandle<T> WithPlayableDirector<T>(this TweenHandle<T> tweenHandle, PlayableDirector playableDirector, float duration)
            where T : unmanaged, ITweenBuilder
        {
            return tweenHandle.WithPlayableDirector(playableDirector, playableDirector.playableAsset, duration);
        }
        
        public static TweenHandle<T> WithPlayableDirector<T>(this TweenHandle<T> tweenHandle, PlayableDirector playableDirector, PlayableAsset playableAsset)
            where T : unmanaged, ITweenBuilder
        {
            return tweenHandle.WithPlayableDirector(playableDirector, playableAsset, (float)playableAsset.duration);
        }
        
        public static TweenHandle<T> WithPlayableDirector<T>(this TweenHandle<T> tweenHandle, PlayableDirector playableDirector, PlayableAsset playableAsset, float duration)
            where T : unmanaged, ITweenBuilder
        {
            tweenHandle.WithDuration(duration);

            if (!playableDirector.playableGraph.IsValid())
            {
                playableDirector.RebuildGraph();
            }
            
            tweenHandle.TweenBuilder.AddComponentObject(tweenHandle.Entity, new TweenPlayableDirectorOnPlay()
            {
                PlayableDirector = playableDirector,
                PlayableAsset = playableAsset,
            });

            return tweenHandle;
        }
    }
}