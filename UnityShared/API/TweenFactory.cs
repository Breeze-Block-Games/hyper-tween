using System;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using BreezeBlockGames.HyperTween.UnityShared.SequenceBuilders;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.API
{
    public struct TweenFactory<TTweenBuilder> where TTweenBuilder : unmanaged, ITweenBuilder
    {
        private bool _withJournaling;
        private TTweenBuilder _tweenBuilder;

        public TTweenBuilder TweenBuilder => _tweenBuilder;

        public TweenFactory(TTweenBuilder tweenBuilder, bool withJournaling = false)
        {
            _tweenBuilder = tweenBuilder;
            _withJournaling = withJournaling;
        }

        public void SetJournalingEnabled(bool state)
        {
            _withJournaling = state;
        }

        public TweenHandle<TTweenBuilder> CreateNullTween()
        {
            return new TweenHandle<TTweenBuilder>(Entity.Null, this);
        }

        public TweenHandle<TTweenBuilder> CreateTween(in FixedString64Bytes name = default)
        {
            var tweenHandle = new TweenHandle<TTweenBuilder>(
                _tweenBuilder.CreateEntity(),
                this
            );

            if (!name.IsEmpty)
            {
                _tweenBuilder.SetName(tweenHandle.Entity, name);
            }

            // This is the bare minimum that non-null tweens need
            _tweenBuilder.AddComponent<TweenDurationOverflow>(tweenHandle.Entity);

            if (_withJournaling)
            {
                tweenHandle = tweenHandle.EnsureHasJournaling();
            }
            
            return tweenHandle;
        }

        public BatchTweenHandle<TTweenBuilder> CreateBatch(NativeArray<Entity> targetEntities)
        {
            var batchTweenHandle = new BatchTweenHandle<TTweenBuilder>(targetEntities, _tweenBuilder);
      
            // TODO: Flags for batches
            batchTweenHandle.BatchTweenBuilder.AddComponent<TweenParameter>();

            return batchTweenHandle;
        }

        public TweenHandle<TTweenBuilder> GetBuilder(TweenHandle tweenHandle)
        {
            return new TweenHandle<TTweenBuilder>(tweenHandle.Entity, this);
        }

        public NativeArray<TweenHandle<TTweenBuilder>> AllocateNativeArray(int capacity, Allocator allocator)
        {
            return new NativeArray<TweenHandle<TTweenBuilder>>(capacity, allocator);
        }
        
        public TweenHandle Serial(in FixedString64Bytes name, NativeArray<TweenHandle> subTweens, Allocator allocator = Allocator.Temp)
        {
            return Serial(in name, allocator)
                .Append(subTweens)
                .Build();
        }
        
        public SequenceFactory<TTweenBuilder, SerialSequenceBuilder<TTweenBuilder>> Serial(in FixedString64Bytes name, Allocator allocator = Allocator.Temp)
        {
            return new SequenceFactory<TTweenBuilder, SerialSequenceBuilder<TTweenBuilder>>(
                in name,
                this,
                new SerialSequenceBuilder<TTweenBuilder>(),
                allocator);
        }
        
        public TweenHandle Parallel(in FixedString64Bytes name, NativeArray<TweenHandle> subTweens, Allocator allocator = Allocator.Temp)
        {
            return Parallel(in name, allocator)
                .Append(subTweens)
                .Build();
        }

        public SequenceFactory<TTweenBuilder, ParallelSequenceBuilder<TTweenBuilder>> Parallel(in FixedString64Bytes name, Allocator allocator = Allocator.Temp)
        {
            return new SequenceFactory<TTweenBuilder, ParallelSequenceBuilder<TTweenBuilder>>(
                in name,
                this, 
                new ParallelSequenceBuilder<TTweenBuilder>(),
                allocator);
        }
        
        public TweenHandle Stagger(in FixedString64Bytes name, NativeArray<TweenHandle> subTweens, float delayPerTween, Allocator allocator = Allocator.Temp)
        {
            return Stagger(in name, delayPerTween, allocator)
                .Append(subTweens)
                .Build();
        }
        
        public SequenceFactory<TTweenBuilder, StaggerSequenceBuilder<TTweenBuilder>> Stagger(in FixedString64Bytes name, float delayPerTween, Allocator allocator = Allocator.Temp)
        {
            return new SequenceFactory<TTweenBuilder, StaggerSequenceBuilder<TTweenBuilder>>(
                in name,
                this,
                new StaggerSequenceBuilder<TTweenBuilder>(delayPerTween),
                allocator);
        }
    }
}