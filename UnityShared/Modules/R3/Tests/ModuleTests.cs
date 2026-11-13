#if UNITY_INCLUDE_TESTS
using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using NUnit.Framework;
using Unity.Core;
using Unity.Entities;
using UnityEngine;
using BreezeBlockGames.HyperTween.UnityShared.Modules.R3.API;
using R3;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.R3.Tests
{
    public class ModuleTests
    {
        private World _world = null!;
        private TweenFactory<EntityManagerTweenBuilder> _factory;
        private double _elapsed;
        [SetUp] public void SetUp() { _world = DefaultWorldInitialization.Initialize("ModuleTweenTests"); _world.GetExistingSystemManaged<UpdateWorldTimeSystem>().Enabled = false; _factory = _world.EntityManager.CreateTweenFactory(); _elapsed = 0; }
        [TearDown] public void TearDown() { _world.Dispose(); }
        private void Tick(float delta) { _elapsed += delta; _world.SetTime(new TimeData(_elapsed, delta)); _world.Update(); _world.EntityManager.CompleteAllTrackedJobs(); }
        private void Drain() { for (var i=0;i<8;i++) Tick(0); }
        [Test] public void ReactiveInteger_IntermediateAndFinalValues()
        {
            using var value = new ReactiveProperty<int>(0);
            _factory.CreateTween().IntReactivePropertyTo(value,100).WithDuration(1).Play();
            Tick(.25f); Assert.That(value.Value, Is.EqualTo(25)); Tick(1); Drain(); Assert.That(value.Value, Is.EqualTo(100));
        }
    }
}
#endif
