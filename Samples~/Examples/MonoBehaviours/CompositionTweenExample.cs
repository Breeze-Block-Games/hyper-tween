using System.Linq;
using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.InvokeAction.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms.API;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using UnityEngine;

namespace HyperTween.Examples
{
    public class CompositionTweenExample : MonoBehaviour
    {
        private void Start()
        {
            var factory = HyperTweenFactory.Get();
            
            var transforms = Enumerable.Range(1, 3)
                .Select(_ => GameObject.CreatePrimitive(PrimitiveType.Cube).transform)
                .ToArray();

            // Create a serial sequence factory to add child tweens to
            var serial = factory.Serial("Serial");
            for (var i = 0; i < 3; i++)
            {
                var randomPositionsTween = CreateRandomPositionsTween(factory, transforms);
                
                serial.Append(randomPositionsTween);
            }

            // Builds the serial sequence factory so that it becomes a regular tween
            serial.Build()
                // Destroys all the transforms when the serial sequence stops
                .InvokeActionOnStop(_ =>
                {
                    foreach (var t in transforms)
                    {
                        Destroy(t.gameObject);
                    }
                })
                .Play();
        }

        private static TweenHandle CreateRandomPositionsTween<T>(TweenFactory<T> factory, Transform[] transforms) 
            where T : unmanaged, ITweenBuilder
        {
            // Creates a tween for each transform that moves it to a random position
            var tweens = transforms
                .Select(t => CreateRandomPositionTween(factory, t));

            // Creates a parallel sequence so that all the transforms move at the same time
            return factory.Parallel("Parallel")
                .Append(tweens)
                .Build();
        }
        
        private static TweenHandle CreateRandomPositionTween<T>(TweenFactory<T> factory, Transform t) 
            where T : unmanaged, ITweenBuilder
        {
            return factory.CreateTween("MoveTransform")
                .WithDuration(1f)
                .WithTransform(t)
                .WithLocalPositionOutput(Random.onUnitSphere);
        }
    }
}