using System;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Systems
{
    public readonly struct InstanceIdKey : IEquatable<InstanceIdKey>
    {
        private readonly int _instanceId;
        private readonly ComponentType _componentType;

        public InstanceIdKey(int instanceId, ComponentType componentType)
        {
            _instanceId = instanceId;
            _componentType = componentType;
        }

        public bool Equals(InstanceIdKey other)
        {
            return _instanceId == other._instanceId && _componentType == other._componentType;
        }

        public override int GetHashCode()
        {
            // Using bitwise operations and arithmetic to combine the hash codes
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + _instanceId;
                hash = hash * 31 + _componentType.GetHashCode();
                return hash;
            }            
        }
    }
}