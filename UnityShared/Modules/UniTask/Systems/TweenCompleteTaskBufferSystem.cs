using System.Collections.Generic;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems;
using Cysharp.Threading.Tasks;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.UniTask.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateBefore(typeof(PreTweenStructuralChangeECBSystem))]
    public class TweenCompleteTaskBufferSystem : ComponentSystemBase
    {
        private readonly List<UniTaskCompletionSource> _taskCompletionSources = new();
        
        public void CompleteTask(UniTaskCompletionSource tcs)
        {
            _taskCompletionSources.Add(tcs);
        }
        
        public override void Update()
        {
            foreach (var taskCompletionSource in _taskCompletionSources)
            {
                taskCompletionSource.TrySetResult();
            }
            _taskCompletionSources.Clear();
        }
    }
}