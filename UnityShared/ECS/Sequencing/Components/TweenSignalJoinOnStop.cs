using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Sequencing.Components
{
    public struct TweenSignalJoinOnStop : IComponentData
    {
        public Entity Target;
    }
}