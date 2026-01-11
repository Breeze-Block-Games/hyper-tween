#define HYPERTWEEN_STACKTRACES

using System;
using System.Diagnostics;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Systems;
using Unity.Entities;
using Unity.Transforms;

namespace BreezeBlockGames.HyperTween.UnityShared.API
{
    public readonly struct TweenHandle
    {
        private readonly Entity _entity;
        private readonly WorldUnmanaged _worldUnmanaged;

        internal TweenHandle(Entity entity, WorldUnmanaged worldUnmanaged)
        {
            _entity = entity;
            _worldUnmanaged = worldUnmanaged;
        }

        public Entity Entity => _entity;
        public WorldUnmanaged World => _worldUnmanaged;

        public bool IsNull => _entity.Equals(Entity.Null) || !World.IsCreated;
        
        public TweenHandle<EntityCommandBufferTweenBuilder> GetBuilder()
        {
            return World.CreateTweenFactory().GetBuilder(this);
        }
        
        public TweenHandle<TBuilder> GetBuilder<TBuilder>(TweenFactory<TBuilder> factory) 
            where TBuilder : unmanaged, ITweenBuilder
        {
            return factory.GetBuilder(this);
        }

        public void Play()
        {
            if (IsNull)
            {
                // It's a null tween, playing it wouldn't do anything anyway!
                return;
            }
            
            GetBuilder().Play();
        }
    }


    public readonly struct TweenHandle<TBuilder> where TBuilder : unmanaged, ITweenBuilder
    {
        [Flags]
        private enum FeatureFlags
        {
            None = 0,
            NonNull = 1 << 0,
            Timer = 1 << 1,
            Parameter = 1 << 2,
            LocalTransform = 1 << 3,
            Journaling = 1 << 4
        }
        
        private readonly Entity _entity;
        private readonly TweenFactory<TBuilder> _factory;
        private readonly FeatureFlags _featureFlags;
        
        public Entity Entity
        {
            get
            {
                if (IsNull)
                {
                    throw new InvalidOperationException($"Attempting to access Entity for a Null Tween. " +
                                                        $"Either use {nameof(IsNull)} to check for validity first, or call {nameof(EnsureNonNull)}");
                }
                
                return _entity;
            }
        }

        public TBuilder TweenBuilder => _factory.TweenBuilder;
        
        public bool IsNull => _entity.Equals(Entity.Null);

        internal TweenHandle(Entity entity, TweenFactory<TBuilder> factory)
        {
            _entity = entity;
            _factory = factory;
            _featureFlags = entity.Equals(Entity.Null) ? FeatureFlags.None : FeatureFlags.NonNull;
        }

        private TweenHandle(Entity entity, TweenFactory<TBuilder> factory, FeatureFlags featureFlags)
        {
            _entity = entity;
            _factory = factory;
            _featureFlags = featureFlags;
        }

        public static implicit operator TweenHandle(TweenHandle<TBuilder> b) => new(b._entity, b._factory.TweenBuilder.WorldUnmanaged);

        public TweenHandle<TBuilder> EnsureNonNull()
        {
            if (!IsNull)
            {
                return this;
            }
            
            // Just suppresses read-only warning
            var factory = _factory;
                
            return factory.CreateTween();
        }

        private TweenHandle<TBuilder> EnsureHas<TComponentData>(FeatureFlags featureFlags) 
            where TComponentData : unmanaged, IComponentData
        {
            // We already have the requested featureFlags, no need to continue
            if (Has(featureFlags))
            {
                return this;
            }

            var tweenHandle = EnsureNonNull();
            
            _factory.TweenBuilder.AddComponent<TComponentData>(_entity);

            return tweenHandle.WithFeatureFlags(featureFlags);
        }
        
        public TweenHandle<TBuilder> EnsureHasTimer()
        {
            return EnsureHas<TweenTimer>(FeatureFlags.Timer);
        }
        
        public TweenHandle<TBuilder> EnsureHasParameter()
        {
            return EnsureHas<TweenParameter>(FeatureFlags.Parameter);
        }
        
        public TweenHandle<TBuilder> EnsureHasLocalTransform()
        {
            return EnsureHas<LocalTransform>(FeatureFlags.LocalTransform);
        }
        
        internal TweenHandle<TBuilder> EnsureHasJournaling()
        {
#if HYPER_TWEEN_ENABLE_JOURNAL
            return EnsureHas<TweenJournal>(FeatureFlags.Journaling);
#else
            throw new InvalidOperationException("Attempting to create a Tween with journalling but HYPER_TWEEN_ENABLE_JOURNAL is not defined");
#endif
        }
        
        private bool Has(FeatureFlags featureFlags)
        {
            return (_featureFlags & featureFlags) == featureFlags;
        }

        private TweenHandle<TBuilder> WithFeatureFlags(FeatureFlags featureFlags)
        {
            return new TweenHandle<TBuilder>(
                Entity, 
                _factory,
                _featureFlags | featureFlags
            );
        }
    }
}