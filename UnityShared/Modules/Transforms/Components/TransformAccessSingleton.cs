using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine.Jobs;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms.Components
{
    public struct TransformAccessSingleton : IComponentData, IDisposable
    {
        [NativeDisableUnsafePtrRestriction]
        public TransformAccessArray TransformAccessArray;
        public NativeList<Entity> EntityLookup;
        public NativeHashMap<Entity, int> IndexLookup;
        
        public void Dispose()
        {
            TransformAccessArray.Dispose();
            EntityLookup.Dispose();
            IndexLookup.Dispose();
        }
    }
}