using Unity.Entities;
using UnityEngine.Scripting;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems
{
    [UpdateInGroup(typeof(TweenStructuralChangeSystemGroup))]
    // Allow Tweens to naturally stop before we attempt to start any, otherwise we may get false positive conflicts
    [UpdateAfter(typeof(OnTweenStopSystemGroup))]
    public partial class OnTweenPlaySystemGroup : ComponentSystemGroup
    {
        [Preserve]
        public OnTweenPlaySystemGroup()
        {
        }
    }
}