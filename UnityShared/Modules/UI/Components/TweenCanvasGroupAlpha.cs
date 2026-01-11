using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Attributes;
using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[assembly: RegisterUnityEngineComponentType(typeof(CanvasGroup))]

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.UI.Components
{
    [DetectConflicts(typeof(UnityObjectInstanceId))]
    public struct TweenCanvasGroupAlpha : ITweenTo<CanvasGroup, float>
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

        public float Read(in CanvasGroup component)
        {
            return component.alpha;
        }

        public void Write(ref CanvasGroup component, float value)
        {
            component.alpha = value;
        }
    }
}