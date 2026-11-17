#if UNITY_INCLUDE_TESTS
using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using NUnit.Framework;
using Unity.Core;
using Unity.Entities;
using UnityEngine;
using BreezeBlockGames.HyperTween.UnityShared.Modules.Timeline.API;
using UnityEngine.Playables;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.Timeline.Tests
{
    public class ModuleTests
    {
        private World _world = null!;
        private World _previousWorld = null!;
        private TweenFactory<EntityManagerTweenBuilder> _factory;
        private double _elapsed;
        [SetUp] public void SetUp()
        {
            _previousWorld = World.DefaultGameObjectInjectionWorld;
            try
            {
                _world = DefaultWorldInitialization.Initialize("ModuleTweenTests");
                _world.GetExistingSystemManaged<UpdateWorldTimeSystem>().Enabled = false;
                _factory = _world.EntityManager.CreateTweenFactory();
                _elapsed = 0;
            }
            catch
            {
                TearDown();
                throw;
            }
        }
        [TearDown] public void TearDown()
        {
            try
            {
                if (_world != null && _world.IsCreated && !ReferenceEquals(_world, _previousWorld))
                {
                    try { ScriptBehaviourUpdateOrder.RemoveWorldFromCurrentPlayerLoop(_world); }
                    finally { _world.Dispose(); }
                }
            }
            finally
            {
                World.DefaultGameObjectInjectionWorld = _previousWorld != null && _previousWorld.IsCreated ? _previousWorld : null;
                _world = null!;
            }
        }
        private void Tick(float delta) { _elapsed += delta; _world.SetTime(new TimeData(_elapsed, delta)); _world.Update(); _world.EntityManager.CompleteAllTrackedJobs(); }
        private void Drain() { for (var i=0;i<8;i++) Tick(0); }
        public class TestBehaviour : PlayableBehaviour {}
        public class TestPlayableAsset : PlayableAsset
        {
            public override double duration => 2;
            public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) => ScriptPlayable<TestBehaviour>.Create(graph);
        }
        [Test] public void Director_UsesTweenDurationForGraphSpeed()
        {
            var go = new GameObject("director", typeof(PlayableDirector)); var asset = ScriptableObject.CreateInstance<TestPlayableAsset>();
            try {
                var director=go.GetComponent<PlayableDirector>(); director.timeUpdateMode=DirectorUpdateMode.Manual;
                _factory.CreateTween().WithPlayableDirector(director,asset,1).Play(); Tick(.25f);
                Assert.That(director.playableGraph.IsValid(), Is.True);
                Assert.That(director.playableGraph.GetRootPlayable(0).GetSpeed(), Is.EqualTo(2));
            } finally { Object.DestroyImmediate(go); Object.DestroyImmediate(asset); }
        }
        [Test] public void Director_DestroyedBeforePlay_PreservesSequenceTime()
        {
            var go = new GameObject("director", typeof(PlayableDirector)); var asset = ScriptableObject.CreateInstance<TestPlayableAsset>();
            try {
                var first = _factory.CreateTween().WithPlayableDirector(go.GetComponent<PlayableDirector>(),asset,.1f);
                var next = _factory.CreateTween().WithDuration(1);
                _factory.Serial("destroyed director").Append(first).Append(next).Build().Play();
                Object.DestroyImmediate(go); Tick(.25f); Drain();
                Assert.That(_world.EntityManager.GetComponentData<BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components.TweenTimer>(next.Entity).Value, Is.EqualTo(.15f).Within(.0001));
            } finally { if (go) Object.DestroyImmediate(go); Object.DestroyImmediate(asset); }
        }
    }
}
#endif
