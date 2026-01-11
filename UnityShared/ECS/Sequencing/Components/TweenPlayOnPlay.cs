using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Sequencing.Components
{
    public struct TweenPlayOnPlay : IComponentData
    {
        public Entity Target;
    }
}