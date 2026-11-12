namespace BreezeBlockGames.HyperTween.UnityShared.API
{
    public static class CollectionTweenBuilderExtensions
    {
        public static void Collect(this TweenHandle<CollectionTweenBuilder> tweenHandle, int sortOrder = 0)
        {
            tweenHandle.TweenBuilder.TweenCollection.AddTween(sortOrder, tweenHandle);
        } 
    }
}