using Unity.Entities;
using Unity.Profiling;
using UnityEngine.Scripting;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class TweenStructuralChangeSystemGroup : ComponentSystemGroup, IRateManager
    {
        public struct TimeAdvance : IComponentData
        {
            public ulong Value;
        }
        private Entity _timeAdvanceEntity;
        private ulong _timeAdvance;
        private bool _isDirty;

        public void MarkDirty()
        {
            _isDirty = true;
        }

        // TODO: This should use a Singleton/System version checks
        public void MarkClean()
        {
            _isDirty = false;
        }

        [Preserve]
        public TweenStructuralChangeSystemGroup()
        {
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            RateManager = this;
            _timeAdvanceEntity = EntityManager.CreateEntity(typeof(TimeAdvance));
        }

        protected override void OnUpdate()
        {
            // Zero-delta drain updates belong to the preceding logical time advance.
            if (World.Time.DeltaTime != 0f)
                EntityManager.SetComponentData(_timeAdvanceEntity, new TimeAdvance { Value = ++_timeAdvance });
            // We always need to Update at least once, because we might be using EntityManager to play Tweens
            // There is potential to avoid this, by _always_ playing Tweens via PreTweenStructuralChangeECBSystem
            MarkDirty();

            using (new ProfilerMarker("TweenStructuralChangeSystemGroup").Auto())
            {
                base.OnUpdate();
            }
        }

        public bool ShouldGroupUpdate(ComponentSystemGroup group)
        {
            var isDirty = _isDirty;
            _isDirty = false;
            return isDirty;
        }

        public float Timestep
        {
            get; set;
        }
    }
}