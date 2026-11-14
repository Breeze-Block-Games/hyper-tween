#if UNITY_INCLUDE_TESTS
using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using NUnit.Framework;
using Unity.Core;
using Unity.Entities;
using UnityEngine;
using BreezeBlockGames.HyperTween.UnityShared.Modules.UniTask.API;
using Cysharp.Threading.Tasks;

namespace BreezeBlockGames.HyperTween.UnityShared.Modules.UniTask.Tests
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
        [Test] public void PlayAsync_CompletesAfterStopAndNullCompletesImmediately()
        {
            var task = _factory.CreateTween().WithDuration(.1f).PlayAsync();
            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Pending)); Tick(.2f); Drain();
            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded)); task.GetAwaiter().GetResult();
            Assert.That(_factory.CreateNullTween().PlayAsync().Status, Is.EqualTo(UniTaskStatus.Succeeded));
        }
    }
}
#endif
