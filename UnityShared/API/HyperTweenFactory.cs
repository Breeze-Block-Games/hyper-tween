using BreezeBlockGames.HyperTween.UnityShared.SequenceBuilders;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.API
{
    public static class HyperTweenFactory
    {
#if HYPER_TWEEN_ENABLE_JOURNAL && HYPER_TWEEN_ENABLE_JOURNAL_BY_DEFAULT
        public const bool JournalingByDefault = true;
#else
        public const bool JournalingByDefault = false;
#endif
        
        private static TweenFactory<EntityCommandBufferTweenBuilder> _tweenFactory;
        
        public static TweenFactory<EntityCommandBufferTweenBuilder> Get(bool withJournaling = JournalingByDefault)
        {
            return World.DefaultGameObjectInjectionWorld.CreateTweenFactory(withJournaling);
        }
        
        public static TweenHandle<EntityCommandBufferTweenBuilder> CreateTween(in FixedString64Bytes name = default, bool withJournaling = JournalingByDefault)
        {
            return Get(withJournaling).CreateTween(in name);
        }

        public static SequenceFactory<EntityCommandBufferTweenBuilder, ParallelSequenceBuilder<EntityCommandBufferTweenBuilder>> Parallel(in FixedString64Bytes name, bool withJournaling = JournalingByDefault)
        {
            return Get(withJournaling).Parallel(in name);
        }
        
        public static SequenceFactory<EntityCommandBufferTweenBuilder, SerialSequenceBuilder<EntityCommandBufferTweenBuilder>> Serial(in FixedString64Bytes name, bool withJournaling = JournalingByDefault)
        {
            return Get(withJournaling).Serial(in name);
        }
        
        public static SequenceFactory<EntityCommandBufferTweenBuilder, StaggerSequenceBuilder<EntityCommandBufferTweenBuilder>> Stagger(in FixedString64Bytes name, float delayPerTween, bool withJournaling = JournalingByDefault)
        {
            return Get(withJournaling).Stagger(in name, delayPerTween);
        }
    }
}