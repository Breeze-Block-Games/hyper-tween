using System;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Systems
{
    public readonly struct EntityTypeKey : IEquatable<EntityTypeKey>
    {
        private readonly Entity _targetEntity;
        private readonly ComponentType _componentType;

        public EntityTypeKey(Entity targetEntity, ComponentType componentType)
        {
            _targetEntity = targetEntity;
            _componentType = componentType;
        }

        public bool Equals(EntityTypeKey other)
        {
            return _targetEntity == other._targetEntity && _componentType == other._componentType;
        }

        public override int GetHashCode()
        {
            // Using bitwise operations and arithmetic to combine the hash codes
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + _targetEntity.GetHashCode();
                hash = hash * 31 + _componentType.GetHashCode();
                return hash;
            }            
        }
    }
}