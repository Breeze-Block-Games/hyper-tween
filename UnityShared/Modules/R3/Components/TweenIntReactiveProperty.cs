using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Attributes;
using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using Unity.Mathematics;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.R3.Components
{
    [DetectConflicts(typeof(ObjectHashCode))]
    public struct TweenIntReactiveProperty : ITweenTo<IntReactivePropertyComponent, int>
    {
        public int Value;
        
        public int GetValue()
        {
            return Value;
        }

        public int Lerp(int from, int to, float parameter)
        {
            return (int)math.round(math.lerp(from, to, parameter));
        }

        public int Read(in IntReactivePropertyComponent component)
        {
            return component.Value.Value;
        }

        public void Write(ref IntReactivePropertyComponent component, int value)
        {
            component.Value.Value = value;
        }
    }
}