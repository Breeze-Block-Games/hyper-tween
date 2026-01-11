using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components
{
    public interface ITweenFrom<TValue> : IComponentData
    {
        public TValue GetValue();
        public void SetValue(TValue value);
    }
}