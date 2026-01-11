using BreezeBlockGames.HyperTween.UnityShared.ECS.Invoke.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using BreezeBlockGames.HyperTween.UnityShared.Modules.InvokeAction.Components;
using UnityEngine;
using UnityEngine.Playables;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.Timeline.Components
{
    [UpdateAfter(typeof(TweenInvokeAction))]
    public class TweenPlayableDirector : ITweenInvokeOnPlay
    {
        public UnityEngine.Playables.PlayableDirector PlayableDirector;
        public PlayableAsset PlayableAsset;
        
        public void Invoke(in TweenDuration tweenDuration)
        {
            if (!PlayableDirector.gameObject.activeInHierarchy)
            {
                Debug.LogError($"Cannot play disabled {nameof(PlayableDirector)}: {PlayableDirector.name}", PlayableDirector);
                return;
            }

            PlayableDirector.Play(PlayableAsset);
            
            if (!PlayableDirector.playableGraph.IsValid())
            {
                PlayableDirector.RebuildGraph();
            }

            PlayableDirector.playableGraph.GetRootPlayable(0).SetSpeed(PlayableAsset.duration / tweenDuration.Value);
        }
    }
}