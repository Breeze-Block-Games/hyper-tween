using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Invoke.Systems
{
    public interface ITweenInvokeJob<TJobData> : IJobChunk where TJobData : unmanaged
    {
        public TJobData JobData { get; set; }
    }
}