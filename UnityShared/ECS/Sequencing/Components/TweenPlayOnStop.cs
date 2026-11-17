using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Sequencing.Components
{
    public struct TweenPlayOnStop : IComponentData
    {
        public Entity Target;
    }
}