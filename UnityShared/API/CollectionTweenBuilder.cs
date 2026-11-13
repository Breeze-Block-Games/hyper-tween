using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.API
{
    public struct CollectionTweenBuilder : ITweenBuilder
    {
        internal readonly TweenCollection TweenCollection;

        private EntityCommandBufferTweenBuilder _subTweenBuilder;

        public CollectionTweenBuilder(EntityCommandBufferTweenBuilder subTweenBuilder, TweenCollection tweenCollection)
        {
            _subTweenBuilder = subTweenBuilder;
            TweenCollection = tweenCollection;
        }

        public WorldUnmanaged WorldUnmanaged => _subTweenBuilder.WorldUnmanaged;

        public Entity CreateEntity()
        {
            var entity = _subTweenBuilder.CreateEntity();
            TweenCollection.TrackCreatedEntity(entity, WorldUnmanaged);
            return entity;
        }

        public void DestroyEntity(Entity entity)
        {
            _subTweenBuilder.DestroyEntity(entity);
            TweenCollection.MarkDestroyedEntity(entity, WorldUnmanaged);
        }

        public void Instantiate(Entity prefab, NativeArray<Entity> entities)
        {
            _subTweenBuilder.Instantiate(prefab, entities);
            foreach (var entity in entities)
                TweenCollection.TrackCreatedEntity(entity, WorldUnmanaged);
        }

        public void SetName(Entity entity, in FixedString64Bytes name)
        {
            _subTweenBuilder.SetName(entity, name);
        }

        public void AddComponent<TComponent>(Entity e) where TComponent : unmanaged, IComponentData
        {
            _subTweenBuilder.AddComponent<TComponent>(e);
        }

        public void AddComponent<TComponent>(NativeArray<Entity> entities) where TComponent : unmanaged, IComponentData
        {
            _subTweenBuilder.AddComponent<TComponent>(entities);
        }

        public void AddComponent<TComponent>(Entity e, TComponent componentData) where TComponent : unmanaged, IComponentData
        {
            _subTweenBuilder.AddComponent(e, componentData);
        }

        public void AddComponent<TComponent>(NativeArray<Entity> entities, TComponent componentData) where TComponent : unmanaged, IComponentData
        {
            _subTweenBuilder.AddComponent(entities, componentData);
        }

        public void AddComponentObject<TComponent>(Entity e, TComponent componentData) where TComponent : class
        {
            _subTweenBuilder.AddComponentObject(e, componentData);
        }

        public DynamicBuffer<TBufferElement> AddBuffer<TBufferElement>(Entity e) where TBufferElement : unmanaged, IBufferElementData
        {
            return _subTweenBuilder.AddBuffer<TBufferElement>(e);
        }
    }
}