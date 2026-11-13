using System;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.TweenBuilders
{
    public struct BatchTweenBuilder<TTweenBuilder> : IBatchTweenBuilder where TTweenBuilder : ITweenBuilder
    {
        private TTweenBuilder _tweenBuilder;
        [ReadOnly] private NativeArray<Entity> _entities;
        public int Count => _entities.Length;

        public BatchTweenBuilder(NativeArray<Entity> entities, TTweenBuilder tweenBuilder)
        {
            _entities = entities;
            _tweenBuilder = tweenBuilder;
        }

        public int GetRangeCount(int sourceLength, int sourceIndex, int destinationIndex, int count)
        {
            if (sourceIndex < 0 || sourceIndex > sourceLength)
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            if (destinationIndex < 0 || destinationIndex > Count)
                throw new ArgumentOutOfRangeException(nameof(destinationIndex));
            if (count == -1)
                count = sourceLength - sourceIndex;
            if (count < 0 || count > sourceLength - sourceIndex || count > Count - destinationIndex)
                throw new ArgumentOutOfRangeException(nameof(count));
            return count;
        }

        public void SetNames(NativeArray<FixedString64Bytes> names) => SetNames(names, 0, 0);
        public void SetNames(NativeArray<FixedString64Bytes> names, int sourceIndex, int destinationIndex, int count = -1)
        {
            count = GetRangeCount(names.Length, sourceIndex, destinationIndex, count);
            for (var i = 0; i < count; i++)
                _tweenBuilder.SetName(_entities[destinationIndex + i], names[sourceIndex + i]);
        }
        public void AddComponent<TComponent>() where TComponent : unmanaged, IComponentData => AddComponentRange<TComponent>(0, Count);
        public void AddComponentRange<TComponent>(int destinationIndex, int count) where TComponent : unmanaged, IComponentData
        {
            count = GetRangeCount(Count, destinationIndex, destinationIndex, count);
            if (count == 0)
                return;
            if (destinationIndex == 0 && count == Count)
            {
                _tweenBuilder.AddComponent<TComponent>(_entities);
                return;
            }
            for (var i = 0; i < count; i++)
                _tweenBuilder.AddComponent<TComponent>(_entities[destinationIndex + i]);
        }
        public void AddComponent<TComponent>(TComponent componentData) where TComponent : unmanaged, IComponentData
        {
            _tweenBuilder.AddComponent(_entities, componentData);
        }
        public void AddComponents<TComponent>(NativeArray<TComponent> componentDatas) where TComponent : unmanaged, IComponentData => AddComponents(componentDatas, 0, 0);
        public void AddComponents<TComponent>(NativeArray<TComponent> componentDatas, int sourceIndex, int destinationIndex, int count = -1) where TComponent : unmanaged, IComponentData
        {
            count = GetRangeCount(componentDatas.Length, sourceIndex, destinationIndex, count);
            for (var i = 0; i < count; i++)
                _tweenBuilder.AddComponent(_entities[destinationIndex + i], componentDatas[sourceIndex + i]);
        }
        public void AddComponentObjects<TComponent>(TComponent[] componentDatas) where TComponent : class => AddComponentObjects(componentDatas, 0, 0);
        public void AddComponentObjects<TComponent>(TComponent[] componentDatas, int sourceIndex, int destinationIndex, int count = -1) where TComponent : class
        {
            if (componentDatas == null)
                throw new ArgumentNullException(nameof(componentDatas));
            count = GetRangeCount(componentDatas.Length, sourceIndex, destinationIndex, count);
            for (var i = 0; i < count; i++)
                _tweenBuilder.AddComponentObject(_entities[destinationIndex + i], componentDatas[sourceIndex + i]);
        }
    }
}
