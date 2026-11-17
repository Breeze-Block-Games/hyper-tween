using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.Auto.Components;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using Cysharp.Threading.Tasks;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.UniTask.API
{
    public static class TweenUniTaskExtensions
    {
        public static Cysharp.Threading.Tasks.UniTask PlayAsync(this TweenHandle tweenHandle)
        {
            if (tweenHandle.IsNull)
            {
                return Cysharp.Threading.Tasks.UniTask.CompletedTask;
            }
            
            return tweenHandle.GetBuilder().PlayAsync();
        }
        
        public static Cysharp.Threading.Tasks.UniTask PlayAsync<TBuilder>(this TweenHandle<TBuilder> tweenHandle) where TBuilder : unmanaged, ITweenBuilder
        {
            if (tweenHandle.IsNull)
            {
                return Cysharp.Threading.Tasks.UniTask.CompletedTask;
            }
            
            var tcs = new UniTaskCompletionSource();

            tweenHandle
                .Play()
                .TweenBuilder.AddComponentObject(tweenHandle.Entity, new TweenCompleteTaskOnStop()
                {
                    TaskCompletionSource = tcs
                });
            
            return tcs.Task;
        }
    }
}