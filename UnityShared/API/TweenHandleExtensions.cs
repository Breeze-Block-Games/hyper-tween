using BreezeBlockGames.HyperTween.UnityShared.ECS.Sequencing.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.API
{
    public static class TweenHandleExtensions
    {
        public static TweenHandle<TBuilder> WithTarget<TBuilder>(this TweenHandle<TBuilder> tweenHandle, Entity entity) 
            where TBuilder : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle.EnsureNonNull();
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenTarget()
            {
                Target = entity
            });

            return tweenHandle;
        }

        public static TweenHandle<TBuilder> WithDuration<TBuilder>(this TweenHandle<TBuilder> tweenHandle, float duration) 
            where TBuilder : unmanaged, ITweenBuilder
        {
            if (duration <= 0f)
            {
                return tweenHandle;
            }

            tweenHandle = tweenHandle.EnsureHasTimer();
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenDuration()
            {
                Value = duration,
                InverseValue = 1f / duration
            });

            return tweenHandle;
        }

        public static TweenHandle<TBuilder> WithEaseIn<TBuilder>(this TweenHandle<TBuilder> tweenHandle, float strength = 1f) 
            where TBuilder : unmanaged, ITweenBuilder
        {
            return tweenHandle.WithHermiteEasing(0, strength);
        }
        
        public static TweenHandle<TBuilder> WithEaseOut<TBuilder>(this TweenHandle<TBuilder> tweenHandle, float strength = 1f) 
            where TBuilder : unmanaged, ITweenBuilder
        {
            return tweenHandle.WithHermiteEasing(strength, 0);
        }
        
        public static TweenHandle<TBuilder> WithEaseInOut<TBuilder>(this TweenHandle<TBuilder> tweenHandle) 
            where TBuilder : unmanaged, ITweenBuilder
        {
            return tweenHandle.WithHermiteEasing(0, 0);
        }
        
        public static TweenHandle<TBuilder> WithHermiteEasing<TBuilder>(this TweenHandle<TBuilder> tweenHandle, float m0, float m1) 
            where TBuilder : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle.EnsureHasParameter();
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenHermiteEasing(m0, m1));
            return tweenHandle;
        }

        public static TweenHandle<TBuilder> Play<TBuilder>(this TweenHandle<TBuilder> tweenHandle, float skipDuration) 
            where TBuilder : unmanaged, ITweenBuilder
        {
            if (tweenHandle.IsNull)
            {
                return tweenHandle;
            }
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenRequestPlaying()
            {
                DurationOverflow = skipDuration
            });

            return tweenHandle;
        }
        
        public static TweenHandle<TBuilder> Play<TBuilder>(this TweenHandle<TBuilder> tweenHandle) 
            where TBuilder : unmanaged, ITweenBuilder
        {
            if (tweenHandle.IsNull)
            {
                return tweenHandle;
            }
            
            tweenHandle.TweenBuilder.AddComponent<TweenRequestPlaying>(tweenHandle.Entity);

            return tweenHandle;
        }
        
        internal static TweenHandle<TBuilder> PlayOnPlay<TBuilder>(this TweenHandle<TBuilder> tweenHandle, TweenHandle targetTweenHandle) 
            where TBuilder : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle.EnsureNonNull();

            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenPlayOnPlay()
            {
                Target = targetTweenHandle.Entity
            });

            return tweenHandle;
        }

        internal static TweenHandle<TBuilder> PlayOnStop<TBuilder>(this TweenHandle<TBuilder> tweenHandle, TweenHandle targetTweenHandle) 
            where TBuilder : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle.EnsureNonNull();

            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenPlayOnStop()
            {
                Target = targetTweenHandle.Entity
            });

            return tweenHandle;
        }
        
        internal static TweenHandle<TBuilder> ForkOnPlay<TBuilder>(this TweenHandle<TBuilder> tweenHandle, NativeArray<TweenHandle> targetTweenHandles)  
            where TBuilder : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle.EnsureNonNull();

            var buffer = tweenHandle.TweenBuilder.AddBuffer<TweenForkOnPlay>(tweenHandle.Entity);
            foreach (var targetTweenHandle in targetTweenHandles)
            {
                buffer.Add(new TweenForkOnPlay()
                {
                    Target = targetTweenHandle.Entity
                });
            }

            return tweenHandle;
        }
        
        internal static TweenHandle<TBuilder> StopOnJoin<TBuilder>(this TweenHandle<TBuilder> tweenHandle, TweenHandle targetTweenHandle) 
            where TBuilder : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle.EnsureNonNull();

            tweenHandle.TweenBuilder.AddComponent(targetTweenHandle.Entity, new TweenSignalJoinOnStop()
            {
                Target = tweenHandle.Entity
            });
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenStopOnJoin()
            {
                RequiredSignals = 1
            });

            return tweenHandle;
        }
        
        internal static TweenHandle<TBuilder> StopOnJoin<TBuilder>(this TweenHandle<TBuilder> tweenHandle, NativeArray<TweenHandle> targetTweenHandles) 
            where TBuilder : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle.EnsureNonNull();

            var count = 0;
            
            foreach (var targetTweenHandle in targetTweenHandles)
            {
                tweenHandle.TweenBuilder.AddComponent(targetTweenHandle.Entity, new TweenSignalJoinOnStop()
                {
                    Target = tweenHandle.Entity
                });

                count++;
            }
            
            tweenHandle.TweenBuilder.AddComponent(tweenHandle.Entity, new TweenStopOnJoin()
            {
                RequiredSignals = count
            });

            return tweenHandle;
        }

        public static TweenHandle<TBuilder> DestroyTargetOnStop<TBuilder>(this TweenHandle<TBuilder> tweenHandle) 
            where TBuilder : unmanaged, ITweenBuilder
        {
            tweenHandle = tweenHandle.EnsureNonNull();

            tweenHandle.TweenBuilder.AddComponent<TweenDestroyTargetOnStop>(tweenHandle.Entity);
            
            return tweenHandle;
        }
        
        public static TweenHandle<TBuilder> WithJournaling<TBuilder>(this TweenHandle<TBuilder> tweenHandle)
            where TBuilder : unmanaged, ITweenBuilder
        {
            return tweenHandle.EnsureHasJournaling();
        }

        public static BatchTweenHandle<TBuilder> CreateBatch<TBuilder>(this TweenHandle<TBuilder> tweenHandle, int numTweens, Allocator allocator) 
            where TBuilder : unmanaged, ITweenBuilder
        {
            var batchTweenHandle = new BatchTweenHandle<TBuilder>(tweenHandle.Entity, numTweens, allocator, tweenHandle.TweenBuilder);
            
            // TODO: Flags for batches
            batchTweenHandle.BatchTweenBuilder.AddComponent<TweenParameter>();

            return batchTweenHandle;
        }
    }
}