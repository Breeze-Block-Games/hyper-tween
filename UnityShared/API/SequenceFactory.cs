using System.Collections.Generic;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using Unity.Collections;

namespace BreezeBlockGames.HyperTween.UnityShared.API
{
    public struct SequenceFactory<TTweenBuilder, TSequenceBuilder>
        where TTweenBuilder : unmanaged, ITweenBuilder
        where TSequenceBuilder : ISequenceBuilder<TTweenBuilder>
    {
        private readonly Allocator _allocator;
        private readonly FixedString64Bytes _name;
        
        private TweenFactory<TTweenBuilder> _tweenFactory;
        private TSequenceBuilder _sequenceBuilder;
        private NativeList<TweenHandle> _subTweens;
        
        public SequenceFactory(in FixedString64Bytes name, TweenFactory<TTweenBuilder> tweenFactory, TSequenceBuilder sequenceBuilder, Allocator allocator)
        {
            _name = name;
            _tweenFactory = tweenFactory;
            _sequenceBuilder = sequenceBuilder;
            _allocator = allocator;
            
            _subTweens = new NativeList<TweenHandle>(5, allocator);
        }
        
        public SequenceFactory<TTweenBuilder, TSequenceBuilder> Append(IEnumerable<TweenHandle> subTweens)
        {
            foreach (var subTween in subTweens)
            {
                if (subTween.IsNull)
                {
                    continue;
                }
                
                Append(subTween);
            }

            return this;
        }
        
        public SequenceFactory<TTweenBuilder, TSequenceBuilder> Append(NativeArray<TweenHandle> subTweens)
        {
            foreach (var subTween in subTweens)
            {
                if (subTween.IsNull)
                {
                    continue;
                }
                
                Append(subTween);
            }

            return this;
        }

        public SequenceFactory<TTweenBuilder, TSequenceBuilder> Append(TweenHandle? subTween)
        {
            if (!subTween.HasValue || subTween.Value.IsNull)
            {
                return this;
            }
            
            _subTweens.Add(subTween.Value);
            return this;
        }
                
        public SequenceFactory<TTweenBuilder, TSequenceBuilder> Append(TweenHandle subTween)
        {
            if (subTween.IsNull)
            {
                return this;
            }
            
            _subTweens.Add(subTween);
            return this;
        }

        public TweenHandle<TTweenBuilder> Build()
        {
            try
            {
                return _subTweens.Length switch
                {
                    0 => _tweenFactory.CreateNullTween(),
                    1 => _tweenFactory.GetBuilder(_subTweens[0]),
                    _ => _sequenceBuilder.Build(in _name, _tweenFactory, _subTweens, _allocator)
                };
            }
            finally
            {
                _subTweens.Dispose();
            }
        }
    }
}