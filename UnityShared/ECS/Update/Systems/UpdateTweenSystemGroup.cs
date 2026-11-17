using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems;
using Unity.Entities;
using Unity.Transforms;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TweenStructuralChangeSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial class UpdateTweenSystemGroup : ComponentSystemGroup
    {
    }
}