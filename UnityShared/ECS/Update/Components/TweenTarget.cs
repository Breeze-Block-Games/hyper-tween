using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components
{
    public struct TweenTarget : IComponentData
    {
        public Entity Target;
    }
}