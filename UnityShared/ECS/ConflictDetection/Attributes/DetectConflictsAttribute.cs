using System;
using JetBrains.Annotations;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Attributes
{
    public class DetectConflictsAttribute : Attribute
    {
        [UsedImplicitly]
        public Type? TargetInstanceIdComponentType;

        public DetectConflictsAttribute(Type targetInstanceIdComponentType)
        {
            TargetInstanceIdComponentType = targetInstanceIdComponentType;
        }
    }
}