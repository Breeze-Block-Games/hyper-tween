using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems;
using BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.Systems
{
    [UpdateInGroup(typeof(OnTweenPlaySystemGroup), OrderFirst = true)]
    public partial struct InitializeTweenLocalTransformsSystem : ISystem
    {
        private EntityQuery _required, _missing;
        public void OnCreate(ref SystemState state)
        {
            _required = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenInitializeLocalTransform, TweenOnPlay>());
            _missing = state.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TweenInitializeLocalTransform, TweenOnPlay>().WithNone<LocalTransform>());
            state.RequireForUpdate(_required);
        }
        public void OnUpdate(ref SystemState state)
        {
            // Query only missing components: AddComponent(data) would reset supplied transforms.
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            ecb.AddComponent(_missing, LocalTransform.Identity);
            ecb.RemoveComponent<TweenInitializeLocalTransform>(_required, EntityQueryCaptureMode.AtPlayback);
            ecb.Playback(state.EntityManager);
        }
    }
}
