using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Attributes;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using Unity.Mathematics;
using Unity.Transforms;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.Components
{
    [DetectConflicts(typeof(TransformInstanceId))]
    public struct TweenLocalPosition : ITweenTo<LocalTransform, float3>
    {
        public float3 Value;

        public float3 GetValue()
        {
            return Value;
        }

        public float3 Lerp(float3 from, float3 to, float parameter)
        {
            return math.lerp(from, to, parameter);
        }

        public readonly float3 Read(in LocalTransform component)
        {
            return component.Position;
        }

        public readonly void Write(ref LocalTransform component, float3 value)
        {
            component.Position = value;
        }
    }
}