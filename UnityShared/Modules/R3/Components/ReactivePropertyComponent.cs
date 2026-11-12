using R3;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.R3.Components
{
    public class ReactivePropertyComponent<TValue> : IComponentData
    {
        public ReactiveProperty<TValue> Value = null!;
    }
}