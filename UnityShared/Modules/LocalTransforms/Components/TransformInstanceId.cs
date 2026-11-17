using System;
using JetBrains.Annotations;
using Unity.Entities;
using UnityEngine;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.Components
{
    public struct TransformInstanceId : IComponentData
    {
        // This gets reinterpreted by AddToConflictLookupSystemJob
        [UsedImplicitly]
        public EntityId Value;

        public static TransformInstanceId Create(Transform transform)
        {
            var entityId = transform.GetEntityId();
            
            if (!entityId.IsValid())
            {
                throw new InvalidOperationException($"Transform has non-valid EntityId: {transform}");
            }

            return new TransformInstanceId()
            {
                Value = entityId
            };
        }
    }
}