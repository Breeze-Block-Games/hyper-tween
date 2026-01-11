using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms.API;
using UnityEngine;

namespace HyperTween.Examples
{
    public class FromTweenExample : MonoBehaviour
    {
        void Start()
        {
            HyperTweenFactory.CreateTween("FromTweenExample")
                .WithDuration(1f)
                .WithTransform(transform)
                .WithLocalPositionOutput(from: new Vector3(1, 2, 3), to: new Vector3(2, 3, 4))
                .Play();
        }
    }
}