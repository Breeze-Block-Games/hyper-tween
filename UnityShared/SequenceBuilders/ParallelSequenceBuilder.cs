using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Systems;
using Unity.Collections;
using Unity.Transforms;

namespace BreezeBlockGames.HyperTween.UnityShared.SequenceBuilders
{
    public struct ParallelSequenceBuilder<TTweenBuilder> : ISequenceBuilder<TTweenBuilder> where TTweenBuilder : unmanaged, ITweenBuilder
    {
        public TweenHandle<TTweenBuilder> Build(in FixedString64Bytes name, TweenFactory<TTweenBuilder> tweenFactory, NativeList<TweenHandle> subTweens, Allocator allocator)
        {
            var tweenHandle = tweenFactory.CreateTween(in name);

            return tweenHandle
                .ForkOnPlay(subTweens.AsArray())
                .StopOnJoin(subTweens.AsArray());
        }
    }
}