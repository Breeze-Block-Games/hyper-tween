using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using Unity.Collections;

namespace BreezeBlockGames.HyperTween.UnityShared.SequenceBuilders
{
    public struct StaggerSequenceBuilder<TTweenBuilder> : ISequenceBuilder<TTweenBuilder> where TTweenBuilder : unmanaged, ITweenBuilder
    {
        private float _delayPerTween;

        public StaggerSequenceBuilder(float delayPerTween)
        {
            _delayPerTween = delayPerTween;
        }

        public TweenHandle<TTweenBuilder> Build(in FixedString64Bytes name, TweenFactory<TTweenBuilder> tweenFactory, NativeList<TweenHandle> subTweens, Allocator allocator)
        {
            var parallelBuilder = tweenFactory.Parallel(in name, allocator);

            var currentDelay = 0f;
            
            foreach (var subTween in subTweens)
            {
                parallelBuilder
                    .Append(tweenFactory.Serial(in name, allocator)
                        .Append(tweenFactory
                            .CreateTween("StaggerDelay")
                            .WithDuration(currentDelay))
                        .Append(subTween)
                        .Build());

                currentDelay += _delayPerTween;
            }

            return parallelBuilder.Build();
        }
    }
}