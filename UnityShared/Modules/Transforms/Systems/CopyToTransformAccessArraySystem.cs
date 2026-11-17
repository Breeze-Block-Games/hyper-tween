using BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TransformSystemGroup))]
    public partial struct CopyToTransformAccessArraySystem : ISystem
    {
        [BurstCompile]
        struct Job : IJobParallelForTransform
        {
            [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorld;
            [ReadOnly] public NativeList<Entity> Entities;

            public void Execute(int index, TransformAccess transform)
            {
                if (!transform.isValid)
                {
                    return;
                }
                
                var ltw = LocalToWorld[Entities[index]];
                
                transform.SetLocalPositionAndRotation(ltw.Position, ltw.Rotation);
                transform.localScale = ltw.Value.Scale();
            }
        }

        private ComponentLookup<LocalToWorld> _localToWorkLookup;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TransformAccessSingleton>();
            
            _localToWorkLookup = state.GetComponentLookup<LocalToWorld>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var singleton = SystemAPI.GetSingleton<TransformAccessSingleton>();

            _localToWorkLookup.Update(ref state);

            state.Dependency = new Job()
            {
                LocalToWorld = _localToWorkLookup,
                Entities = singleton.EntityLookup
            }.Schedule(singleton.TransformAccessArray, state.Dependency);
        }
    }
}