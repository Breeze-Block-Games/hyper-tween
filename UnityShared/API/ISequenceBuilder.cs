using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using Unity.Collections;

namespace BreezeBlockGames.HyperTween.UnityShared.API
{
    public interface ISequenceBuilder<TTweenBuilder> where TTweenBuilder : unmanaged, ITweenBuilder
    {
        TweenHandle<TTweenBuilder> Build(in FixedString64Bytes name,
            TweenFactory<TTweenBuilder> tweenFactory,
            NativeList<TweenHandle> subTweens,
            Allocator allocator);
    }
}