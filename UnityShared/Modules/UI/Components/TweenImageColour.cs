using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Attributes;
using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using UnityEngine;
using UnityEngine.UI;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.UI.Components
{
    [DetectConflicts(typeof(UnityObjectInstanceId))]
    public struct TweenImageColour : ITweenTo<Image, Color>
    {
        public Color Value;

        public Color GetValue()
        {
            return Value;
        }

        public Color Lerp(Color from, Color to, float parameter)
        {
            return Color.Lerp(from, to, parameter);
        }

        public readonly Color Read(in Image component)
        {
            return component.color;
        }

        public readonly void Write(ref Image component, Color value)
        {
            component.color = value;
        }
    }
}