using System;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.API
{
    public struct BatchTweenHandle<T> : IDisposable
        where T : ITweenBuilder
    {
        private BatchTweenBuilder<T> _batchTweenBuilder;

        [ReadOnly]
        private NativeArray<Entity> _entities;

        private readonly bool _disposeEntities;

        public BatchTweenHandle(Entity prefab, int count, Allocator allocator, T tweenBuilder)
        {
            tweenBuilder.AddComponent<Prefab>(prefab);

            _entities = new NativeArray<Entity>(count, allocator);

            tweenBuilder.Instantiate(prefab, _entities);

            // No longer need the prefab
            tweenBuilder.DestroyEntity(prefab);

            _batchTweenBuilder = new BatchTweenBuilder<T>(_entities, tweenBuilder);

            _disposeEntities = true;
        }

        public BatchTweenHandle(NativeArray<Entity> entities, T tweenBuilder)
        {
            _entities = entities;
            _batchTweenBuilder = new BatchTweenBuilder<T>(_entities, tweenBuilder);
            _disposeEntities = false;
        }

        public BatchTweenBuilder<T> BatchTweenBuilder => _batchTweenBuilder;

        public BatchTweenHandle<T> WithNames(NativeArray<FixedString64Bytes> names, int sourceIndex = 0, int destinationIndex = 0, int count = -1)
        {
            _batchTweenBuilder.SetNames(names, sourceIndex, destinationIndex, count);
            return this;
        }

        public BatchTweenHandle<T> WithTargets(NativeArray<Entity> entities, int sourceIndex = 0, int destinationIndex = 0, int count = -1)
        {
            _batchTweenBuilder.AddComponents(entities.Reinterpret<TweenTarget>(), sourceIndex, destinationIndex, count);
            return this;
        }

        public BatchTweenHandle<T> WithDuration(float duration)
        {
            _batchTweenBuilder.AddComponent<TweenTimer>();
            _batchTweenBuilder.AddComponent(TweenDuration.Create(duration));
            _batchTweenBuilder.AddComponent<TweenDurationOverflow>();

            return this;
        }

        /// <summary>Legacy source offset; selected durations start at destination zero.</summary>
        public BatchTweenHandle<T> WithDurations(NativeArray<float> durations, int startIndex = 0)
            => WithDurations(durations, startIndex, 0);

        public BatchTweenHandle<T> WithDurations(NativeArray<float> durations, int sourceIndex, int destinationIndex, int count = -1)
        {
            count = _batchTweenBuilder.GetRangeCount(durations.Length, sourceIndex, destinationIndex, count);
            var tweenDurations = new NativeArray<TweenDuration>(count, Allocator.Temp);
            try
            {
                for (var i = 0; i < count; i++)
                    tweenDurations[i] = TweenDuration.Create(durations[sourceIndex + i]);
                _batchTweenBuilder.AddComponentRange<TweenTimer>(destinationIndex, count);
                _batchTweenBuilder.AddComponentRange<TweenDurationOverflow>(destinationIndex, count);
                _batchTweenBuilder.AddComponents(tweenDurations, 0, destinationIndex, count);
            }
            finally { tweenDurations.Dispose(); }
            return this;
        }

        public BatchTweenHandle<T> Play()
        {
            _batchTweenBuilder.AddComponent<TweenRequestPlaying>();
            return this;
        }

        public void Dispose()
        {
            if (_disposeEntities)
            {
                _entities.Dispose();
            }
        }
    }
}