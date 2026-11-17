#if HYPER_TWEEN_ENABLE_LIFETIME_VALIDATION
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Validation.Systems
{
    public struct ValidateQueryUnchanged : IDisposable
    {
        private struct CopyJob : IJob
        {
            public NativeList<Entity> From;
            public NativeHashSet<Entity> To;
            
            public void Execute()
            {
                foreach (var entity in From)
                {
                    To.Add(entity);
                }
            }
        }
        
        private struct ValidateJob : IJob
        {
            public FixedString128Bytes AdditionalLogFormat;
            public FixedString128Bytes MissingLogFormat;
            public NativeList<Entity> From;
            public NativeHashSet<Entity> To;
            
            public void Execute()
            {
                var errorString = new FixedString512Bytes();
                
                foreach (var entity in From)
                {
                    if (!To.Remove(entity))
                    {
                        errorString.Clear();
                        errorString.AppendFormat(AdditionalLogFormat, entity.ToFixedString());
                        
                        Debug.LogError(errorString);
                    }
                }
                From.Clear();

                foreach (var entity in To)
                {
                    errorString.Clear();
                    errorString.AppendFormat(MissingLogFormat, entity.ToFixedString());
                        
                    Debug.LogError(errorString);
                }
                
                To.Clear();
            }
        }
        
        public FixedString128Bytes AdditionalLogFormat;
        public FixedString128Bytes MissingLogFormat;
        public NativeHashSet<Entity> EntitiesInQuery;

        public static ValidateQueryUnchanged Create(string additionalLogFormat, string missingLogFormat)
        {
            return new ValidateQueryUnchanged()
            {
                AdditionalLogFormat = additionalLogFormat,
                MissingLogFormat = missingLogFormat,
                EntitiesInQuery = new NativeHashSet<Entity>(16, Allocator.Persistent),
            };
        }

        public JobHandle Populate(EntityQuery entityQuery, JobHandle inputDeps)
        {
            var toPlay = entityQuery
                .ToEntityListAsync(Allocator.TempJob, inputDeps, out var getEntitiesJob);
            
            var copyJob = new CopyJob()
            {
                From = toPlay,
                To = EntitiesInQuery
            }.Schedule(getEntitiesJob);

            return toPlay.Dispose(copyJob);
        }
        
        public JobHandle Validate(EntityQuery entityQuery, JobHandle inputDeps)
        {
            var toPlay = entityQuery
                .ToEntityListAsync(Allocator.TempJob, inputDeps, out var getEntitiesJob);
            
            var validateJob = new ValidateJob()
            {
                AdditionalLogFormat = AdditionalLogFormat,
                MissingLogFormat = MissingLogFormat,
                From = toPlay,
                To = EntitiesInQuery
            }.Schedule(getEntitiesJob);

            return toPlay.Dispose(validateJob);
        }

        public void Dispose()
        {
            EntitiesInQuery.Dispose();
        }
    }
}
#endif