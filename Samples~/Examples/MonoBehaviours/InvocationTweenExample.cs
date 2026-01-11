using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.InvokeAction.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms.API;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using UnityEngine;

namespace HyperTween.Examples
{
    public class InvocationTweenExample : MonoBehaviour
    {
        void Start()
        {
            CreateTween(HyperTweenFactory.Get());
        }

        void CreateTween<T>(TweenFactory<T> tweenFactory) where T : unmanaged, ITweenBuilder
        {
            tweenFactory.CreateTween("InvocationTweenExample")
                .WithDuration(1f)
                .WithTransform(transform)
                .WithLocalPositionOutput(5f * Random.onUnitSphere)
                .WithEaseInOut()
                .WithJournaling()
                .InvokeActionOnPlay(_ =>
                {
                    Debug.Log("Tween played!");
                })
                .InvokeActionOnStop(context =>
                {
                    Debug.Log("Tween stopped!");
                    CreateTween(context.TweenFactory);
                })
                .Play();
        }
    }
}