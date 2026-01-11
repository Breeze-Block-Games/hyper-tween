using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Sequencing.Components
{
    public struct TweenStopOnJoin : IComponentData
    {
        public int CurrentSignals, RequiredSignals;
    }
}