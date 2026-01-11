using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms.API;
using UnityEngine;

namespace HyperTween.Examples
{
    public class EaseTweenExample : MonoBehaviour
    {
        void Start()
        {
            HyperTweenFactory.CreateTween("EaseTweenExample")
                .WithDuration(1f)
                .WithTransform(transform)
                .WithEaseIn(strength: 2f)
                .WithEaseOut(strength: 3f)
                .WithEaseInOut()
                .WithHermiteEasing(0f, 3f)
                .WithLocalPositionOutput(to: new Vector3(1, 2, 3))
                .Play();
        }
    }
}