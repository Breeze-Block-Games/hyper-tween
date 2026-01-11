using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using UnityEngine;

namespace HyperTween.Examples
{
    public class FactoryTweenExample : MonoBehaviour
    {
        void Start()
        {
            var factory = HyperTweenFactory.Get();

            CreateTween(factory, "TweenA", 1f);
            CreateTween(factory, "TweenB", 2f);
        }

        private static void CreateTween<T>(TweenFactory<T> factory, string name, float duration) 
            where T : unmanaged, ITweenBuilder
        {
            factory.CreateTween(name)
                .WithDuration(duration)
                .Play();
        }
    }
}