using JetBrains.Annotations;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Components
{
    public struct UnityObjectInstanceId : IComponentData
    {
        // This gets reinterpreted by AddToConflictLookupSystemJob
        [UsedImplicitly]
#if UNITY_6000_4_OR_NEWER
        private UnityEngine.EntityId _value;
#else
        private int _value;
#endif

        public static UnityObjectInstanceId Create<T>(T obj) where T : UnityEngine.Object
        {
            return new UnityObjectInstanceId()
            {
#if UNITY_6000_4_OR_NEWER
                _value = obj.GetEntityId()
#else
                _value = obj.GetInstanceID()
#endif
            };
        }
    }
}