using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Attributes;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using Unity.Mathematics;
using Unity.Transforms;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.Components
{
    [DetectConflicts(typeof(TransformInstanceId))]
    public struct TweenLocalUniformScale : ITweenTo<LocalTransform, float>
    {
        public float Value;

        public float GetValue()
        {
            return Value;
        }

        public float Lerp(float from, float to, float parameter)
        {
            return math.lerp(from, to, parameter);
        }

        public readonly float Read(in LocalTransform component)
        {
            return component.Scale;
        }

        public readonly void Write(ref LocalTransform component, float value)
        {
            component.Scale = value;
        }
    }
}