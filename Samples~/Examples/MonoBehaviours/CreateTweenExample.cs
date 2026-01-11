using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms.API;
using UnityEngine;

namespace HyperTween.Examples
{
    public class CreateTweenExample : MonoBehaviour
    {
        void Start()
        {
            HyperTweenFactory
                // Creates a tween with an optional name that can be used for debugging
                .CreateTween("CreateTweenExample")
                // Sets how long the tween will last
                .WithDuration(1f)
                // Sets which transform to move
                .WithTransform(transform)
                // Sets where the transform should move to
                .WithLocalPositionOutput(new Vector3(1, 2, 3))
                // Plays the tween
                .Play();
        }
    }

}
