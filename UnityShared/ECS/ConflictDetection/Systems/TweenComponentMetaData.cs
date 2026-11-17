using BreezeBlockGames.HyperTween.UnityShared.ECS.Util;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Systems
{
    [BurstCompile]
    public struct TweenComponentMetaData
    {
        [ReadOnly]
        public DynamicTypeInfo TargetComponentTypeInfo;
        [ReadOnly]
        public DynamicTypeInfo InstanceIdComponentTypeInfo;
           
        public bool HasInstanceIdComponent;
           
        public void Initialise<TTarget>(ref SystemState state)
        {
            TargetComponentTypeInfo.InitialiseReadOnly<TTarget>(ref state);
            HasInstanceIdComponent = false;
        }
            
        public void Initialise<TTarget, TInstanceId>(ref SystemState state)
        {
            TargetComponentTypeInfo.InitialiseReadOnly<TTarget>(ref state);
            InstanceIdComponentTypeInfo.InitialiseReadOnly<TInstanceId>(ref state);
            HasInstanceIdComponent = true;
        }
           
        public void Update(ref SystemState state)
        {
            TargetComponentTypeInfo.Update(ref state);
            if(HasInstanceIdComponent)
            {
                InstanceIdComponentTypeInfo.Update(ref state);
            }
        }
    }
}