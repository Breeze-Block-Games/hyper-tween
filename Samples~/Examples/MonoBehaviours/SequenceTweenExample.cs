using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.InvokeAction.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms.API;
using UnityEngine;

namespace HyperTween.Examples
{
    public class SequenceTweenExample : MonoBehaviour
    {
        void Start()
        {
            var factory = HyperTweenFactory.Get(true);

            var serialTweenFactory = factory.Serial("Serial");
            
            serialTweenFactory
                .Append(factory.CreateTween("SequenceTweenExampleA")
                    .WithDuration(5f)
                    .WithTransform(transform)
                    .WithLocalPositionOutput(new Vector3(5f, 0f, 0f)))
                .Append(factory.CreateTween("SequenceTweenExampleB")
                    .WithDuration(0.001f)
                    .WithTransform(transform)
                    .WithLocalPositionOutput(new Vector3(-5f, 0f, 0f)));

            // Build creates a tween like any other, that could be added to another sequence
            var serialTween = serialTweenFactory
                .Build()
                .InvokeActionOnStop(_ => Debug.Log("Serial tween stopped..."));
            
            serialTween.Play();
        }
    }
    
}