#if HYPER_TWEEN_ENABLE_LIFETIME_VALIDATION
using System;
using Unity.Entities;
using Unity.Jobs;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Validation.Systems
{
    public struct TweenLifetimeValidationSingleton : IComponentData, IDisposable
    {
        public ValidateQueryUnchanged ValidatePlay;
        public ValidateQueryUnchanged ValidateStop;

        public static TweenLifetimeValidationSingleton Create()
        {
            return new TweenLifetimeValidationSingleton()
            {
                ValidatePlay = ValidateQueryUnchanged.Create(
                    "TweenRequestPlaying added to non-playing Entity during TweenStructuralChangeSystemGroup execution: {0}",
                    "TweenRequestPlaying removed from non-playing Entity during TweenStructuralChangeSystemGroup execution: {0}"),
                ValidateStop = ValidateQueryUnchanged.Create(
                    "TweenRequestPlaying removed from playing Entity during TweenStructuralChangeSystemGroup execution: {0}",
                    "TweenRequestPlaying added to playing Entity during TweenStructuralChangeSystemGroup execution: {0}"
                )
            };
        }

        public JobHandle Populate(EntityQuery playQuery, EntityQuery stopQuery, JobHandle inputDeps)
        {
            var populatePlay = ValidatePlay.Populate(playQuery, inputDeps);
            var populateStop = ValidateStop.Populate(stopQuery, inputDeps);
            
            return JobHandle.CombineDependencies(populatePlay, populateStop);
        }

        public JobHandle Validate(EntityQuery playQuery, EntityQuery stopQuery, JobHandle inputDeps)
        {
            var populatePlay = ValidatePlay.Validate(playQuery, inputDeps);
            return ValidateStop.Validate(stopQuery, populatePlay);
        }
        
        public void Dispose()
        {
            ValidatePlay.Dispose();
            ValidateStop.Dispose();
        }
    }
}
#endif