using JetBrains.Annotations;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Components
{
    public struct ObjectHashCode : IComponentData
    {
        // This gets reinterpreted by AddToConflictLookupSystemJob
        [UsedImplicitly]
        private int _value;

        public static ObjectHashCode Create<T>(T obj) where T : class
        {
            
            
            return new ObjectHashCode()
            {
                _value = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj)
            };
        }
    }
}