using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components
{
    public struct TweenRequestPlaying : IComponentData
    {
        public float DurationOverflow;
    }
}