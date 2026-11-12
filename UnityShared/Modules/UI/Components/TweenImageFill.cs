using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Attributes;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.UI;

[assembly: RegisterUnityEngineComponentType(typeof(Image))]

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.UI.Components
{
    [DetectConflicts(typeof(Image))]
    public struct TweenImageFill : ITweenTo<Image, float>
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

        public float Read(in Image component)
        {
            return component.fillAmount;
        }

        public void Write(ref Image component, float value)
        {
            component.fillAmount = value;
        }
    }
}