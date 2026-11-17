using BreezeBlockGames.HyperTween.UnityShared.ECS.Invoke.Components;
using BreezeBlockGames.HyperTween.UnityShared.Modules.UniTask.Systems;
using Cysharp.Threading.Tasks;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.UniTask.Components
{
    public class TweenCompleteTask : ITweenInvokeOnStop
    {
        public UniTaskCompletionSource TaskCompletionSource;

        public void Invoke(ref SystemState state)
        {
            // TODO: Make this more performant somehow...
            var tweenCompleteTaskBufferSystem = state.World.GetExistingSystemManaged<TweenCompleteTaskBufferSystem>();
            tweenCompleteTaskBufferSystem.CompleteTask(TaskCompletionSource);
        }
        
    }
}