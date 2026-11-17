#if UNITY_INCLUDE_TESTS
using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using NUnit.Framework;
using Unity.Core;
using Unity.Entities;
using UnityEngine;
using BreezeBlockGames.HyperTween.UnityShared.Modules.UI.API;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.UI.Tests
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
        [Test] public void CanvasGroup_IntermediateAndFinalOutput()
        {
            var go = new GameObject("alpha", typeof(CanvasGroup));
            try { var group = go.GetComponent<CanvasGroup>(); group.alpha=0; _factory.CreateTween().CanvasGroupAlphaTo(group, 1).WithDuration(1).Play(); Tick(.25f); Assert.That(group.alpha, Is.EqualTo(.25f).Within(.0001)); Tick(1); Drain(); Assert.That(group.alpha, Is.EqualTo(1)); }
            finally { Object.DestroyImmediate(go); }
        }
        [Test] public void CanvasGroup_DestroyedBeforePlay_PreservesSequenceTime()
        {
            var go = new GameObject("alpha", typeof(CanvasGroup));
            var first = _factory.CreateTween().CanvasGroupAlphaTo(go.GetComponent<CanvasGroup>(), 1).WithDuration(.1f);
            var next = _factory.CreateTween().WithDuration(1);
            _factory.Serial("destroyed canvas group").Append(first).Append(next).Build().Play();
            Object.DestroyImmediate(go); Tick(.25f); Drain();
            Assert.That(_world.EntityManager.GetComponentData<BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components.TweenTimer>(next.Entity).Value, Is.EqualTo(.15f).Within(.0001));
        }
    }
}
#endif
