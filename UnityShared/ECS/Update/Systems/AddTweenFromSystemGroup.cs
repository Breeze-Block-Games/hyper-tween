using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Systems
{
    [UpdateInGroup(typeof(UpdateTweenSystemGroup))]
    [UpdateBefore(typeof(TweenOutputSystemGroup))]
    public partial class AddTweenFromSystemGroup : ComponentSystemGroup
    {
    }
}