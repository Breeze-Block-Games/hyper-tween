using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Sequencing.Components
{
    [InternalBufferCapacity(4)]
    public struct TweenForkOnPlay : IBufferElementData
    {
        public Entity Target;
    }
}