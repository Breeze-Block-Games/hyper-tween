using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Attributes;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using Unity.Mathematics;
using Unity.Transforms;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.Components
{
    [DetectConflicts(typeof(TransformInstanceId))]
    public struct TweenLocalRotation : ITweenTo<LocalTransform, quaternion>
    {
        public quaternion Value;

        public quaternion GetValue()
        {
            return Value;
        }

        public quaternion Lerp(quaternion from, quaternion to, float parameter)
        {
            return math.slerp(from, to, parameter);
        }

        public readonly quaternion Read(in LocalTransform component)
        {
            return component.Rotation;
        }

        public readonly void Write(ref LocalTransform component, quaternion value)
        {
            component.Rotation = value;
        }
    }
}