using Unity.Entities;
using Unity.Profiling;
using UnityEngine.Scripting;

namespace BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class TweenStructuralChangeSystemGroup : ComponentSystemGroup, IRateManager
    {
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
        }

        protected override void OnUpdate()
        {
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

        public float Timestep { get; set; }
    }
}