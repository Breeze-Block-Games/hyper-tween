using Unity.Entities;
using UnityEngine.Scripting;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems
{
    [UpdateInGroup(typeof(TweenStructuralChangeSystemGroup))]
    [UpdateBefore(typeof(TweenStructuralChangeECBSystem))]
    public partial class OnTweenStopSystemGroup : ComponentSystemGroup
    {
        [Preserve]
        public OnTweenStopSystemGroup()
        {
        }
    }
}