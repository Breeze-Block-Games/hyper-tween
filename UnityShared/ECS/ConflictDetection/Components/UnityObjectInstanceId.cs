using JetBrains.Annotations;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Components
{
    public struct UnityObjectInstanceId : IComponentData
    {
        // This gets reinterpreted by AddToConflictLookupSystemJob
        [UsedImplicitly]
        private int _value;

        public static UnityObjectInstanceId Create<T>(T obj) where T : UnityEngine.Object
        {
            return new UnityObjectInstanceId()
            {
                _value = obj.GetInstanceID()
            };
        }
    }
}