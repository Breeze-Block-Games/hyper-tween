#if UNITY_INCLUDE_TESTS
using BreezeBlockGames.HyperTween.UnityShared.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.InvokeAction.API;
using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Systems;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Structural.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.Components;
using BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms.API;
using BreezeBlockGames.HyperTween.UnityShared.Modules.Transforms.Components;
using BreezeBlockGames.HyperTween.UnityShared.TweenBuilders;
using BreezeBlockGames.HyperTween.UnityShared.Auto;
using BreezeBlockGames.HyperTween.UnityShared.Auto.Components;
using BreezeBlockGames.HyperTween.UnityShared.Modules.LocalTransforms.API;
using NUnit.Framework;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace BreezeBlockGames.HyperTween.UnityShared.Tests
{
    public class RuntimeRegressionTests
    {
        private World _world = null!;
        private World _previousWorld = null!;
        private TweenFactory<EntityManagerTweenBuilder> _factory;
        private double _time;
        [SetUp] public void SetUp()
        {
            _previousWorld = World.DefaultGameObjectInjectionWorld;
            try
            {
                _world = DefaultWorldInitialization.Initialize(nameof(RuntimeRegressionTests));
                _world.GetExistingSystemManaged<UpdateWorldTimeSystem>().Enabled = false;
                _factory = _world.EntityManager.CreateTweenFactory();
                _time = 0;
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
        private void Tick(float delta)
        {
            _time += delta;
            _world.SetTime(new TimeData(_time, delta));
            _world.Update();
            _world.EntityManager.CompleteAllTrackedJobs();
        }
        private void Drain() { for (var i = 0; i < 8; i++) Tick(0); }

        [TestCase(false)] [TestCase(true)]
        public void BatchConstruction_NegativeCountLeavesSourceUnchanged(bool wasPrefab)
        {
            var source = _factory.CreateTween().WithDuration(3);
            if (wasPrefab) _world.EntityManager.AddComponent<Prefab>(source.Entity);
            Assert.Throws<System.ArgumentOutOfRangeException>(() => source.CreateBatch(-1, Allocator.TempJob));
            Assert.That(_world.EntityManager.Exists(source.Entity), Is.True);
            Assert.That(_world.EntityManager.HasComponent<Prefab>(source.Entity), Is.EqualTo(wasPrefab));
            Assert.That(_world.EntityManager.GetComponentData<TweenDuration>(source.Entity).Value, Is.EqualTo(3));
        }

        [Test] public void BatchConstruction_InvalidAllocatorLeavesSourceUnchanged()
        {
            var source = _factory.CreateTween().WithDuration(3);
            Assert.Catch(() => source.CreateBatch(2, Allocator.None));
            Assert.That(_world.EntityManager.Exists(source.Entity), Is.True);
            Assert.That(_world.EntityManager.HasComponent<Prefab>(source.Entity), Is.False);
            Assert.That(_world.EntityManager.GetComponentData<TweenDuration>(source.Entity).Value, Is.EqualTo(3));
        }

        [Test] public void BatchConstruction_NegativeCountRecordsNoPrefabMutation()
        {
            var source = _factory.CreateTween().WithDuration(3);
            using var commands = new EntityCommandBuffer(Allocator.TempJob);
            var builder = new EntityCommandBufferTweenBuilder(_world.Unmanaged, commands);
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                new BatchTweenHandle<EntityCommandBufferTweenBuilder>(source.Entity, -1, Allocator.TempJob, builder));
            commands.Playback(_world.EntityManager);
            Assert.That(_world.EntityManager.Exists(source.Entity), Is.True);
            Assert.That(_world.EntityManager.HasComponent<Prefab>(source.Entity), Is.False);
        }

        public enum BatchFailurePoint { BeforeInstantiation, AfterFirstClone, BeforeSourceDestruction }

        // Only the failing backend boundary is injected. Allocation, component
        // changes, instantiation and rollback still use the actual EntityManager.
        private sealed class FailingBatchBuilder : ITweenBuilder
        {
            private readonly EntityManagerTweenBuilder _inner;
            private readonly Entity _source;
            private readonly BatchFailurePoint _point;
            public readonly System.InvalidOperationException Failure = new("Injected batch construction failure");
            public NativeArray<Entity> Storage;
            public Entity[] Clones = System.Array.Empty<Entity>();
            public FailingBatchBuilder(EntityManagerTweenBuilder inner, Entity source, BatchFailurePoint point)
            {
                _inner = inner; _source = source; _point = point;
            }
            public WorldUnmanaged WorldUnmanaged => _inner.WorldUnmanaged;
            public Entity CreateEntity() => _inner.CreateEntity();
            public void DestroyEntity(Entity entity)
            {
                if (entity == _source && _point == BatchFailurePoint.BeforeSourceDestruction) throw Failure;
                _inner.DestroyEntity(entity);
            }
            public void Instantiate(Entity source, NativeArray<Entity> entities)
            {
                Storage = entities;
                if (_point == BatchFailurePoint.BeforeInstantiation) throw Failure;
                if (_point == BatchFailurePoint.AfterFirstClone)
                {
                    _inner.Instantiate(source, entities.GetSubArray(0, 1));
                    Clones = entities.ToArray();
                    throw Failure;
                }
                _inner.Instantiate(source, entities);
                Clones = entities.ToArray();
            }
            public void SetName(Entity entity, in FixedString64Bytes name) => _inner.SetName(entity, name);
            public void AddComponent<T>(Entity entity) where T : unmanaged, IComponentData => _inner.AddComponent<T>(entity);
            public void AddComponent<T>(NativeArray<Entity> entities) where T : unmanaged, IComponentData => _inner.AddComponent<T>(entities);
            public void AddComponent<T>(Entity entity, T data) where T : unmanaged, IComponentData => _inner.AddComponent(entity, data);
            public void AddComponent<T>(NativeArray<Entity> entities, T data) where T : unmanaged, IComponentData => _inner.AddComponent(entities, data);
            public void AddComponentObject<T>(Entity entity, T data) where T : class => _inner.AddComponentObject(entity, data);
            public DynamicBuffer<T> AddBuffer<T>(Entity entity) where T : unmanaged, IBufferElementData => _inner.AddBuffer<T>(entity);
        }

#if UNITY_EDITOR && ENABLE_UNITY_COLLECTIONS_CHECKS
        // A copied NativeArray retains its pointer after another copy disposes it.
        // Only safety-enabled runtimes can inspect that stale copy safely. Players
        // without these checks must never read or free it after construction.
        private static bool BatchStorageWasReleased(NativeArray<Entity> storage)
        {
            try { _ = storage[0]; return false; }
            catch (System.ObjectDisposedException) { return true; }
        }

#endif

        [TestCase(BatchFailurePoint.BeforeInstantiation)]
        [TestCase(BatchFailurePoint.AfterFirstClone)]
        [TestCase(BatchFailurePoint.BeforeSourceDestruction)]
        public void BatchConstruction_BackendFailureReleasesStorageAndClones(BatchFailurePoint point)
        {
            var source = _factory.CreateTween().WithDuration(3);
            var builder = new FailingBatchBuilder(_factory.TweenBuilder, source.Entity, point);
            try
            {
                var error = Assert.Throws<System.InvalidOperationException>(() =>
                    new BatchTweenHandle<FailingBatchBuilder>(source.Entity, 2, Allocator.TempJob, builder));
                Assert.That(error, Is.SameAs(builder.Failure));
                Assert.That(builder.Storage.IsCreated, Is.True, "The failure must happen after real native allocation.");
#if UNITY_EDITOR && ENABLE_UNITY_COLLECTIONS_CHECKS
                Assert.That(BatchStorageWasReleased(builder.Storage), Is.True, "The unreturned handle must not strand its NativeArray.");
#endif
                Assert.That(_world.EntityManager.Exists(source.Entity), Is.True);
                Assert.That(_world.EntityManager.HasComponent<Prefab>(source.Entity), Is.False);
                Assert.That(_world.EntityManager.GetComponentData<TweenDuration>(source.Entity).Value, Is.EqualTo(3));
                foreach (var clone in builder.Clones)
                    if (clone != Entity.Null) Assert.That(_world.EntityManager.Exists(clone), Is.False, "Failed construction must release created clones.");
            }
            finally
            {
#if UNITY_EDITOR && ENABLE_UNITY_COLLECTIONS_CHECKS
                // Keep the Editor RED run from leaking the exposed storage.
                if (builder.Storage.IsCreated && !BatchStorageWasReleased(builder.Storage)) builder.Storage.Dispose();
#endif
            }
        }

        private struct ParameterFailureState
        {
            public NativeArray<Entity> Storage;
            public Entity FirstClone;
            public Entity SecondClone;
            public bool AddFirstParameter;
        }

        // A pointer to the synchronous test's stack state keeps this backend
        // unmanaged, as required by the public TweenHandle.CreateBatch API.
        private unsafe struct ParameterFailureBuilder : ITweenBuilder
        {
            private readonly EntityManagerTweenBuilder _inner;
            private readonly ParameterFailureState* _state;
            public ParameterFailureBuilder(EntityManagerTweenBuilder inner, ParameterFailureState* state)
            {
                _inner = inner; _state = state;
            }
            public WorldUnmanaged WorldUnmanaged => _inner.WorldUnmanaged;
            public Entity CreateEntity() => _inner.CreateEntity();
            public void DestroyEntity(Entity entity) => _inner.DestroyEntity(entity);
            public void Instantiate(Entity source, NativeArray<Entity> entities)
            {
                _inner.Instantiate(source, entities);
                _state->Storage = entities;
                _state->FirstClone = entities[0];
                _state->SecondClone = entities[1];
            }
            public void SetName(Entity entity, in FixedString64Bytes name) => _inner.SetName(entity, name);
            public void AddComponent<T>(Entity entity) where T : unmanaged, IComponentData => _inner.AddComponent<T>(entity);
            public void AddComponent<T>(NativeArray<Entity> entities) where T : unmanaged, IComponentData
            {
                if (typeof(T) == typeof(TweenParameter))
                {
                    if (_state->AddFirstParameter) _inner.AddComponent<T>(entities[0]);
                    throw new System.InvalidOperationException("Injected batch parameter failure");
                }
                _inner.AddComponent<T>(entities);
            }
            public void AddComponent<T>(Entity entity, T data) where T : unmanaged, IComponentData => _inner.AddComponent(entity, data);
            public void AddComponent<T>(NativeArray<Entity> entities, T data) where T : unmanaged, IComponentData => _inner.AddComponent(entities, data);
            public void AddComponentObject<T>(Entity entity, T data) where T : class => _inner.AddComponentObject(entity, data);
            public DynamicBuffer<T> AddBuffer<T>(Entity entity) where T : unmanaged, IBufferElementData => _inner.AddBuffer<T>(entity);
        }

        [TestCase(false)] [TestCase(true)]
        public unsafe void BatchConstruction_ParameterSetupFailurePreservesSourceAndReleasesClones(bool addFirstParameter)
        {
            var state = new ParameterFailureState { AddFirstParameter = addFirstParameter };
            var factory = new TweenFactory<ParameterFailureBuilder>(new ParameterFailureBuilder(_factory.TweenBuilder, &state));
            var source = factory.CreateTween().WithDuration(3);
            try
            {
                Assert.Throws<System.InvalidOperationException>(() => source.CreateBatch(2, Allocator.TempJob));
                Assert.That(state.Storage.IsCreated, Is.True, "The failure must follow actual clone allocation.");
#if UNITY_EDITOR && ENABLE_UNITY_COLLECTIONS_CHECKS
                Assert.That(BatchStorageWasReleased(state.Storage), Is.True);
#endif
                Assert.That(_world.EntityManager.Exists(source.Entity), Is.True, "Parameter installation is part of creation, before consuming its source.");
                Assert.That(_world.EntityManager.HasComponent<Prefab>(source.Entity), Is.False);
                Assert.That(_world.EntityManager.GetComponentData<TweenDuration>(source.Entity).Value, Is.EqualTo(3));
                Assert.That(_world.EntityManager.Exists(state.FirstClone), Is.False);
                Assert.That(_world.EntityManager.Exists(state.SecondClone), Is.False);
            }
            finally
            {
#if UNITY_EDITOR && ENABLE_UNITY_COLLECTIONS_CHECKS
                if (state.Storage.IsCreated && !BatchStorageWasReleased(state.Storage)) state.Storage.Dispose();
#endif
            }
        }

        [TestCase(0)] [TestCase(2)]
        public void BatchConstruction_OwnedSuccessConsumesSourceAndKeepsClonesAfterDispose(int count)
        {
            var source = _factory.CreateTween().WithDuration(3);
            var batch = source.CreateBatch(count, Allocator.TempJob);
            var disposed = false;
            try
            {
                Assert.That(batch.BatchTweenBuilder.Count, Is.EqualTo(count));
                Assert.That(_world.EntityManager.Exists(source.Entity), Is.False);
                using var query = _world.EntityManager.CreateEntityQuery(typeof(TweenDuration));
                Assert.That(query.CalculateEntityCount(), Is.EqualTo(count));
                using var clones = query.ToEntityArray(Allocator.Temp);
                batch.Dispose();
                disposed = true;
                foreach (var clone in clones)
                {
                    Assert.That(_world.EntityManager.Exists(clone), Is.True, "Handle disposal owns the list, not the tween entities.");
                    Assert.That(_world.EntityManager.HasComponent<Prefab>(clone), Is.False);
                    Assert.That(_world.EntityManager.GetComponentData<TweenDuration>(clone).Value, Is.EqualTo(3));
                }
            }
            finally
            {
                if (!disposed) batch.Dispose();
            }
        }

        [Test] public void BatchConstruction_BorrowedDisposeRetainsCallerStorage()
        {
            using var entities = new NativeArray<Entity>(2, Allocator.TempJob);
            _world.EntityManager.CreateEntity(_world.EntityManager.CreateArchetype(), entities);
            var first = entities[0];
            var batch = _factory.CreateBatch(entities);
            batch.Dispose();
            Assert.That(entities[0], Is.EqualTo(first));
            foreach (var entity in entities) Assert.That(_world.EntityManager.Exists(entity), Is.True);
        }

        [Test] public void BatchConstruction_CommandBufferSuccessMaterializesClones()
        {
            using var commands = new EntityCommandBuffer(Allocator.TempJob);
            var factory = new TweenFactory<EntityCommandBufferTweenBuilder>(new EntityCommandBufferTweenBuilder(_world.Unmanaged, commands));
            using var batch = factory.CreateTween().WithDuration(3).CreateBatch(2, Allocator.TempJob);
            batch.WithDuration(.1f).Play();
            commands.Playback(_world.EntityManager);
            using var query = _world.EntityManager.CreateEntityQuery(typeof(TweenDuration));
            Assert.That(query.CalculateEntityCount(), Is.EqualTo(2));
            using var clones = query.ToEntityArray(Allocator.Temp);
            foreach (var clone in clones) Assert.That(_world.EntityManager.HasComponent<Prefab>(clone), Is.False);
            Tick(.05f); Drain();
            foreach (var clone in clones)
                Assert.That(_world.EntityManager.GetComponentData<TweenTimer>(clone).Value, Is.EqualTo(.05f).Within(.0001));
        }

        [Test] public void TweenCollection_DiscardNestedGroup_ReleasesItsTweensAndPreservesOuterGroup()
        {
            using var collection = TweenCollection.Create();
            collection.Push();
            collection.CreateTweenFactory().CreateTween().WithDuration(2).Collect();
            collection.Push();
            collection.CreateTweenFactory().CreateTween().WithDuration(1).Collect();
            var innerFactory = collection.CreateTweenFactory();
            innerFactory.CreateTween().WithDuration(3); // Partial construction before Collect.
            var first = innerFactory.CreateTween().WithDuration(4);
            var second = innerFactory.CreateTween().WithDuration(5);
            innerFactory.Parallel("discarded compound").Append(first).Append(second).Build().Collect();
            using var ownedBatch = innerFactory.CreateTween().WithDuration(6).CreateBatch(2, Allocator.TempJob);
            var discard = typeof(TweenCollection).GetMethod("Discard", System.Type.EmptyTypes);
            Assert.That(discard, Is.Not.Null, "Rollback needs to discard the current collection group without playing or retaining its entities.");
            discard!.Invoke(collection, null);
            collection.Pop("outer group").Play(); Tick(.25f); Drain();
            using var query = _world.EntityManager.CreateEntityQuery(typeof(TweenDuration));
            Assert.That(query.CalculateEntityCount(), Is.EqualTo(1));
            Assert.That(query.GetSingleton<TweenDuration>().Value, Is.EqualTo(2));
        }
        [Test] public void TweenCollection_DiscardOuterAfterNestedPop_ReleasesCreatedGraph()
        {
            using var collection = TweenCollection.Create();
            collection.Push();
            collection.Push();
            collection.CreateTweenFactory().CreateTween().WithDuration(1).Collect();
            collection.Pop("inner successful binding").Play();
            collection.Discard();
            Tick(.25f); Drain();
            using var query = _world.EntityManager.CreateEntityQuery(typeof(TweenDurationOverflow));
            Assert.That(query.CalculateEntityCount(), Is.Zero);
            Assert.Throws<System.InvalidOperationException>(() => collection.Discard());
        }
        [Test] public void Callbacks_RunOnceOnPlayAndStopAcrossFollowingFrames()
        {
            var plays = 0; var stops = 0;
            _factory.CreateTween().WithDuration(.1f).InvokeActionOnPlay(_ => plays++).InvokeActionOnStop(_ => stops++).Play();
            Tick(.25f); Drain(); Tick(.25f); Drain();
            Assert.That(plays, Is.EqualTo(1)); Assert.That(stops, Is.EqualTo(1));
        }
        [Test] public void Serial_LargeFixedStep_PreservesOverflowAcrossSkippedChild()
        {
            var a = _factory.CreateTween().WithDuration(.1f);
            var b = _factory.CreateTween().WithDuration(.1f);
            var c = _factory.CreateTween().WithDuration(1f);
            _factory.Serial("serial").Append(a).Append(b).Append(c).Build().Play();
            Tick(.25f); Drain();
            Assert.That(_world.EntityManager.GetComponentData<TweenTimer>(c.Entity).Value, Is.EqualTo(.05f).Within(.0001));
            Assert.That(_world.EntityManager.HasComponent<TweenPlaying>(c.Entity), Is.True);
        }
        [TestCase(false)] [TestCase(true)]
        public void Serial_InstantaneousChild_PreservesIncomingRemainder(bool withTransformOutput)
        {
            var target = new GameObject("instant sequence output");
            try
            {
                var stops = 0; var stoppedOverflow = -1f;
                var first = _factory.CreateTween().WithDuration(.1f);
                var instant = _factory.CreateTween().InvokeActionOnStop(context => { stops++; stoppedOverflow = context.TweenDurationOverflow.Value; });
                if (withTransformOutput) instant = instant.WithTransform(target.transform).WithLocalPositionOutput(new float3(3));
                var next = _factory.CreateTween().WithDuration(1);
                _factory.Serial("instant child").Append(first).Append(instant).Append(next).Build().Play();
                Tick(.25f); Drain();
                Assert.That(_world.EntityManager.GetComponentData<TweenTimer>(next.Entity).Value, Is.EqualTo(.15f).Within(.0001));
                Assert.That(stops, Is.EqualTo(1));
                Assert.That(stoppedOverflow, Is.EqualTo(.15f).Within(.0001));
                if (withTransformOutput) Assert.That(target.transform.localPosition, Is.EqualTo(new Vector3(3, 3, 3)));
            }
            finally { Object.DestroyImmediate(target); }
        }
        [Test] public void Parallel_UnequalChildren_JoinForwardsLatestChildRemainder()
        {
            var a = _factory.CreateTween().WithDuration(.1f);
            var b = _factory.CreateTween().WithDuration(.2f);
            var join = _factory.Parallel("parallel").Append(a).Append(b).Build();
            var next = _factory.CreateTween().WithDuration(1f);
            _factory.Serial("serial").Append(join).Append(next).Build().Play();
            Tick(.25f); Drain();
            Assert.That(_world.EntityManager.GetComponentData<TweenTimer>(next.Entity).Value, Is.EqualTo(.05f).Within(.0001));
        }
        [Test] public void Parallel_NestedSerialBranch_RetainsMinimumWithinSameTimeAdvance()
        {
            var direct = _factory.CreateTween().WithDuration(.2f);
            var first = _factory.CreateTween().WithDuration(.1f);
            var second = _factory.CreateTween().WithDuration(.05f);
            var branch = _factory.Serial("nested serial").Append(first).Append(second).Build();
            var parallel = _factory.Parallel("nested join").Append(direct).Append(branch).Build();
            var next = _factory.CreateTween().WithDuration(1);
            _factory.Serial("after nested join").Append(parallel).Append(next).Build().Play();
            Tick(.25f); Drain();
            Assert.That(_world.EntityManager.GetComponentData<TweenTimer>(next.Entity).Value, Is.EqualTo(.05f).Within(.0001));
        }
        [Test] public void Parallel_ChildrenFinishOnDifferentTicks_DiscardsEarlierTickRemainder()
        {
            var first = _factory.CreateTween().WithDuration(.2f);
            var second = _factory.CreateTween().WithDuration(.5f);
            var parallel = _factory.Parallel("different ticks").Append(first).Append(second).Build();
            var next = _factory.CreateTween().WithDuration(1);
            _factory.Serial("after later tick").Append(parallel).Append(next).Build().Play();
            Tick(.21f); Drain(); Tick(.34f); Drain();
            Assert.That(_world.EntityManager.GetComponentData<TweenTimer>(next.Entity).Value, Is.EqualTo(.05f).Within(.0001));
        }
        [Unity.Burst.BurstCompile(CompileSynchronously = true)]
        private unsafe struct ConversionAllocator : AllocatorManager.IAllocator
        {
            public AllocatorManager.AllocatorHandle Handle { get; set; }
            public Allocator ToAllocator => Handle.ToAllocator;
            public bool IsCustomAllocator => Handle.IsCustomAllocator;
            public int Allocations;
            public int Outstanding;
            public AllocatorManager.TryFunction Function => Invoke;
            public void Dispose() => Handle.Dispose();
            public int Try(ref AllocatorManager.Block block)
            {
                var wasAllocated = block.Range.Pointer != System.IntPtr.Zero;
                var owner = block.Range.Allocator;
                block.Range.Allocator = AllocatorManager.Persistent;
                var result = AllocatorManager.Try(ref block);
                block.Range.Allocator = owner;
                if (result == 0)
                {
                    var isAllocated = block.Range.Pointer != System.IntPtr.Zero;
                    if (!wasAllocated && isAllocated) { Allocations++; Outstanding++; }
                    else if (wasAllocated && !isAllocated) Outstanding--;
                }
                return result;
            }
            [Unity.Burst.BurstCompile(CompileSynchronously = true)]
            [AOT.MonoPInvokeCallback(typeof(AllocatorManager.TryFunction))]
            private static int Invoke(System.IntPtr state, ref AllocatorManager.Block block)
                => ((ConversionAllocator*)state)->Try(ref block);
        }
        [Test] public void TransformIdConversion_InvalidLaterElement_DoesNotLeakNativeAllocation()
        {
            var target = new GameObject();
            using var allocator = new AllocatorHelper<ConversionAllocator>(AllocatorManager.Persistent);
            try
            {
                var valid = new[] { target.transform }.ToTransformInstanceIds(allocator.Allocator.ToAllocator);
                try
                {
                    Assert.That(valid.Length, Is.EqualTo(1));
                    Assert.That(allocator.Allocator.Allocations, Is.GreaterThan(0), "Probe must observe actual NativeArray allocation.");
                    Assert.That(allocator.Allocator.Outstanding, Is.GreaterThan(0));
                }
                finally { CollectionHelper.Dispose(valid); }
                Assert.That(allocator.Allocator.Outstanding, Is.Zero);
                Assert.Catch(() => new Transform[] { target.transform, null! }.ToTransformInstanceIds(allocator.Allocator.ToAllocator));
                Assert.That(allocator.Allocator.Outstanding, Is.Zero, "Failed conversion must release its owned NativeArray.");
            }
            finally { Object.DestroyImmediate(target); }
        }
        [Test] public void Batch_DefaultDurations_AddsTimersAndStopsIndependently()
        {
            using var entities = new NativeArray<Entity>(2, Allocator.Temp);
            _world.EntityManager.CreateEntity(_world.EntityManager.CreateArchetype(), entities);
            using var durations = new NativeArray<float>(new[] { .1f, .2f }, Allocator.Temp);
            using var batch = _factory.CreateBatch(entities).WithDurations(durations).Play();
            Assert.That(_world.EntityManager.HasComponent<TweenTimer>(entities[0]), Is.True);
            Assert.That(_world.EntityManager.HasComponent<TweenDurationOverflow>(entities[1]), Is.True);
            Tick(.15f); Drain();
            Assert.That(_world.EntityManager.HasComponent<TweenPlaying>(entities[0]), Is.False);
            Assert.That(_world.EntityManager.HasComponent<TweenPlaying>(entities[1]), Is.True);
            Tick(.1f); Drain();
            Assert.That(_world.EntityManager.HasComponent<TweenPlaying>(entities[1]), Is.False);
        }
        [TestCase(0, 0, 2)] [TestCase(1, 0, 2)] [TestCase(0, 1, 2)] [TestCase(1, 1, 1)] [TestCase(3, 3, 0)]
        public void Batch_DurationRange_UsesIndependentOffsets(int sourceIndex, int destinationIndex, int count)
        {
            using var entities = new NativeArray<Entity>(3, Allocator.Temp);
            _world.EntityManager.CreateEntity(_world.EntityManager.CreateArchetype(), entities);
            using var durations = new NativeArray<float>(new[] { 10f, 20f, 30f }, Allocator.Temp);
            var batch = _factory.CreateBatch(entities);
            // Reflection keeps this regression compilable against the old API, so absence
            // of the required overload is an assertion failure rather than a compile error.
            var method = typeof(BatchTweenHandle<EntityManagerTweenBuilder>).GetMethod("WithDurations", new[] { typeof(NativeArray<float>), typeof(int), typeof(int), typeof(int) });
            Assert.That(method, Is.Not.Null, "Batch durations need independent source/destination offsets and a count.");
            method!.Invoke(batch, new object[] { durations, sourceIndex, destinationIndex, count });
            for (var i = 0; i < entities.Length; i++)
            {
                var selected = i >= destinationIndex && i < destinationIndex + count;
                Assert.That(_world.EntityManager.HasComponent<TweenDuration>(entities[i]), Is.EqualTo(selected));
                Assert.That(_world.EntityManager.HasComponent<TweenTimer>(entities[i]), Is.EqualTo(selected));
                if (selected) Assert.That(_world.EntityManager.GetComponentData<TweenDuration>(entities[i]).Value, Is.EqualTo(durations[sourceIndex + i - destinationIndex]));
            }
        }
        [TestCase(-1, 0, 1)] [TestCase(0, -1, 1)] [TestCase(2, 0, 2)] [TestCase(0, 2, 2)] [TestCase(0, 0, -2)]
        public void Batch_DurationRange_RejectsInvalidRangesBeforeMutation(int sourceIndex, int destinationIndex, int count)
        {
            using var entities = new NativeArray<Entity>(3, Allocator.Temp);
            _world.EntityManager.CreateEntity(_world.EntityManager.CreateArchetype(), entities);
            using var durations = new NativeArray<float>(new[] { 10f, 20f, 30f }, Allocator.Temp);
            var batch = _factory.CreateBatch(entities);
            var method = typeof(BatchTweenHandle<EntityManagerTweenBuilder>).GetMethod("WithDurations", new[] { typeof(NativeArray<float>), typeof(int), typeof(int), typeof(int) });
            Assert.That(method, Is.Not.Null, "Missing range overload");
            var error = Assert.Throws<System.Reflection.TargetInvocationException>(() => method!.Invoke(batch, new object[] { durations, sourceIndex, destinationIndex, count }));
            Assert.That(error!.InnerException, Is.InstanceOf<System.ArgumentOutOfRangeException>());
            foreach (var entity in entities) Assert.That(_world.EntityManager.HasComponent<TweenDuration>(entity), Is.False);
        }
        [TestCase(true)] [TestCase(false)] public void EntityTarget_DestroyedBeforeOrDuringTween_StopsOnTimeAndSequenceContinues(bool beforePlay)
        {
            var target = _world.EntityManager.CreateEntity(typeof(LocalTransform));
            _world.EntityManager.SetComponentData(target, LocalTransform.Identity);
            var first = _factory.CreateTween().WithTarget(target).WithLocalPositionOutput(new float3(1)).WithDuration(.2f);
            var next = _factory.CreateTween().WithDuration(1);
            _factory.Serial("destroyed ECS target").Append(first).Append(next).Build().Play();
            if (!beforePlay) Tick(.1f);
            _world.EntityManager.DestroyEntity(target);
            Tick(beforePlay ? .25f : .15f); Drain();
            Assert.That(_world.EntityManager.GetComponentData<TweenTimer>(next.Entity).Value, Is.EqualTo(.05f).Within(.0001));
        }
        [Test] public void MainThreadCommandBuffer_ManagedCallbacks_RunAtPlayAndStop()
        {
            using var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var factory = new TweenFactory<EntityCommandBufferTweenBuilder>(new EntityCommandBufferTweenBuilder(_world.Unmanaged, ecb));
            var plays = 0; var stops = 0;
            factory.CreateTween().WithDuration(.1f).InvokeActionOnPlay(_ => plays++).InvokeActionOnStop(_ => stops++).Play();
            ecb.Playback(_world.EntityManager); Tick(.25f); Drain();
            Assert.That(plays, Is.EqualTo(1)); Assert.That(stops, Is.EqualTo(1));
        }
        [Test] public void ParallelWriter_ManagedComponents_ReportUnsupportedBackend()
        {
            using var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var builder = new EntityCommandBufferParallelWriterTweenBuilder(_world.Unmanaged, ecb.AsParallelWriter());
            Assert.Throws<System.NotSupportedException>(() => builder.AddComponentObject(Entity.Null, new object()));
        }
        [Test] public void ExclusiveTransaction_ManagedComponentsAndNames_ReportUnsupportedBackend()
        {
            using var staging = new World("isolated transaction capabilities");
            var transaction = staging.EntityManager.BeginExclusiveEntityTransaction();
            try
            {
                var builder = new ExclusiveEntityTransactionTweenBuilder(staging.Unmanaged, transaction);
                var entity = builder.CreateEntity();
                Assert.Throws<System.NotSupportedException>(() => builder.AddComponentObject(entity, new object()));
                Assert.Throws<System.NotSupportedException>(() => builder.SetName(entity, "unsupported name"));
            }
            finally { staging.EntityManager.EndExclusiveEntityTransaction(); }
        }
        [Test] public void Batch_DirectPositionOutput_InitializesSelectedLocalTransform()
        {
            using var entities = new NativeArray<Entity>(3, Allocator.Temp);
            _world.EntityManager.CreateEntity(_world.EntityManager.CreateArchetype(), entities);
            using var durations = new NativeArray<float>(new[] { 1f, 1f, 1f }, Allocator.Temp);
            using var positions = new NativeArray<float3>(new[] { new float3(10), new float3(20), new float3(30) }, Allocator.Temp);
            _factory.CreateBatch(entities).WithDurations(durations, 1, 2, 1).WithLocalPositionOutputs(positions, 1, 2, 1).Play();
            Tick(.25f);
            var actual = _world.EntityManager.GetComponentData<LocalTransform>(entities[2]);
            Assert.That(actual.Position, Is.EqualTo(new float3(5)));
            Assert.That(actual.Rotation, Is.EqualTo(quaternion.identity));
            Assert.That(actual.Scale, Is.EqualTo(1));
        }
        [TestCase(true)] [TestCase(false)]
        public void Batch_DirectRotationAndScaleOutputs_StartFromIdentity(bool rotation)
        {
            using var entities = new NativeArray<Entity>(1, Allocator.Temp);
            _world.EntityManager.CreateEntity(_world.EntityManager.CreateArchetype(), entities);
            using var durations = new NativeArray<float>(new[] { 1f }, Allocator.Temp);
            using var rotations = new NativeArray<quaternion>(new[] { quaternion.RotateY(1) }, Allocator.Temp);
            using var scales = new NativeArray<float>(new[] { 3f }, Allocator.Temp);
            var batch = _factory.CreateBatch(entities).WithDurations(durations);
            if (rotation) batch.WithLocalRotationOutputs(rotations).Play();
            else batch.WithLocalUniformScaleOutputs(scales).Play();
            Tick(.25f);
            var actual = _world.EntityManager.GetComponentData<LocalTransform>(entities[0]);
            Assert.That(actual.Position, Is.EqualTo(float3.zero));
            if (rotation)
            {
                Assert.That(math.distance(actual.Rotation.value, math.slerp(quaternion.identity, rotations[0], .25f).value), Is.LessThan(.0001));
                Assert.That(actual.Scale, Is.EqualTo(1));
            }
            else
            {
                Assert.That(actual.Rotation, Is.EqualTo(quaternion.identity));
                Assert.That(actual.Scale, Is.EqualTo(1.5f).Within(.0001));
            }
        }
        [Test] public void Batch_DirectOutputs_PreserveExistingTransformValues()
        {
            using var entities = new NativeArray<Entity>(1, Allocator.Temp);
            _world.EntityManager.CreateEntity(_world.EntityManager.CreateArchetype(typeof(LocalTransform)), entities);
            var initial = LocalTransform.FromPositionRotationScale(new float3(5), quaternion.RotateY(.4f), 2);
            _world.EntityManager.SetComponentData(entities[0], initial);
            using var durations = new NativeArray<float>(new[] { 1f }, Allocator.Temp);
            using var positions = new NativeArray<float3>(new[] { new float3(20) }, Allocator.Temp);
            using var rotations = new NativeArray<quaternion>(new[] { quaternion.RotateX(1) }, Allocator.Temp);
            using var scales = new NativeArray<float>(new[] { 3f }, Allocator.Temp);
            _factory.CreateBatch(entities).WithDurations(durations).WithLocalPositionOutputs(positions).WithLocalRotationOutputs(rotations).WithLocalUniformScaleOutputs(scales).Play();
            Tick(.25f);
            var actual = _world.EntityManager.GetComponentData<LocalTransform>(entities[0]);
            Assert.That(actual.Position, Is.EqualTo(new float3(8.75f)));
            Assert.That(math.distance(actual.Rotation.value, math.slerp(initial.Rotation, rotations[0], .25f).value), Is.LessThan(.0001));
            Assert.That(actual.Scale, Is.EqualTo(2.25f).Within(.0001));
        }
        [Test] public void Batch_ZeroDurationPositionOutput_WritesFiniteEndpoint()
        {
            using var entities = new NativeArray<Entity>(1, Allocator.Temp);
            _world.EntityManager.CreateEntity(_world.EntityManager.CreateArchetype(), entities);
            var target = _world.EntityManager.CreateEntity(typeof(LocalTransform));
            _world.EntityManager.SetComponentData(target, LocalTransform.Identity);
            using var targets = new NativeArray<Entity>(new[] { target }, Allocator.Temp);
            using var durations = new NativeArray<float>(new[] { 0f }, Allocator.Temp);
            using var froms = new NativeArray<float3>(new[] { float3.zero }, Allocator.Temp);
            using var tos = new NativeArray<float3>(new[] { new float3(1, 2, 3) }, Allocator.Temp);
            _factory.CreateBatch(entities).WithTargets(targets).WithLocalPositionOutputs(froms, tos).WithDurations(durations).Play();
            Tick(.25f); Drain();
            Assert.That(_world.EntityManager.GetComponentData<LocalTransform>(target).Position, Is.EqualTo(new float3(1, 2, 3)));
        }
        [Test] public void Batch_NamesTargetsAndPairedOutputs_ForwardBothOffsets()
        {
            using var entities = new NativeArray<Entity>(4, Allocator.Temp);
            _world.EntityManager.CreateEntity(_world.EntityManager.CreateArchetype(), entities);
            using var names = new NativeArray<FixedString64Bytes>(new FixedString64Bytes[] { "zero", "one", "two" }, Allocator.Temp);
            using var targets = new NativeArray<Entity>(new[] { entities[0], entities[1], entities[3] }, Allocator.Temp);
            using var froms = new NativeArray<float3>(new[] { new float3(10), new float3(20), new float3(30) }, Allocator.Temp);
            using var tos = new NativeArray<float3>(new[] { new float3(11), new float3(21), new float3(31) }, Allocator.Temp);
            _factory.CreateBatch(entities).WithNames(names, 1, 2, 1).WithTargets(targets, 1, 2, 1).WithLocalPositionOutputs(froms, tos, 1, 2, 1);
            _world.EntityManager.GetName(entities[2], out var name);
            Assert.That(name.ToString(), Is.EqualTo("one"));
            Assert.That(_world.EntityManager.GetComponentData<TweenTarget>(entities[2]).Target, Is.EqualTo(entities[1]));
            Assert.That(_world.EntityManager.GetComponentData<TweenLocalPositionFrom>(entities[2]).Value, Is.EqualTo(new float3(20)));
            Assert.That(_world.EntityManager.GetComponentData<TweenLocalPosition>(entities[2]).Value, Is.EqualTo(new float3(21)));
            Assert.That(_world.EntityManager.HasComponent<TweenTarget>(entities[0]), Is.False);
            Assert.That(_world.EntityManager.HasComponent<TweenLocalPosition>(entities[3]), Is.False);
        }
        [Test] public void Batch_PairedOutputInvalidRange_DoesNotAddFirstComponent()
        {
            using var entities = new NativeArray<Entity>(2, Allocator.Temp);
            _world.EntityManager.CreateEntity(_world.EntityManager.CreateArchetype(), entities);
            using var froms = new NativeArray<float3>(2, Allocator.Temp);
            using var tos = new NativeArray<float3>(1, Allocator.Temp);
            var batch = _factory.CreateBatch(entities);
            Assert.Throws<System.ArgumentOutOfRangeException>(() => batch.WithLocalPositionOutputs(froms, tos, 0, 0, 2));
            foreach (var entity in entities) Assert.That(_world.EntityManager.HasComponent<TweenLocalPositionFrom>(entity), Is.False);
        }
        [Test] public void Batch_ManagedTransforms_UsesIndependentOffsetsAndZeroCount()
        {
            using var entities = new NativeArray<Entity>(3, Allocator.Temp);
            _world.EntityManager.CreateEntity(_world.EntityManager.CreateArchetype(), entities);
            var first = new GameObject(); var second = new GameObject();
            try
            {
                using var ids = new NativeArray<TransformInstanceId>(new[] { TransformInstanceId.Create(first.transform), TransformInstanceId.Create(second.transform) }, Allocator.Temp);
                var batch = _factory.CreateBatch(entities).WithManagedTransformOutputs(ids, 2, 3, 0).WithManagedTransformOutputs(ids, 1, 2, 1);
                Assert.That(_world.EntityManager.HasComponent<LocalTransform>(entities[0]), Is.False);
                Assert.That(_world.EntityManager.HasComponent<TransformInstanceId>(entities[0]), Is.False);
                Assert.That(_world.EntityManager.GetComponentData<TransformInstanceId>(entities[2]).Value, Is.EqualTo(ids[1].Value));
            }
            finally { Object.DestroyImmediate(first); Object.DestroyImmediate(second); }
        }
        [Test] public void NullHandle_WithDuration_ConfiguresMaterializedEntity()
        {
            var tween = _factory.CreateNullTween().WithDuration(1f);
            Assert.That(tween.IsNull, Is.False);
            Assert.That(_world.EntityManager.HasComponent<TweenTimer>(tween.Entity), Is.True);
            tween.Play(); Tick(.25f);
            Assert.That(_world.EntityManager.GetComponentData<TweenTimer>(tween.Entity).Value, Is.EqualTo(.25f).Within(.0001));
        }
        [Test] public void NullHandle_DeferredWithDuration_ConfiguresMaterializedEntity()
        {
            using var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var factory = new TweenFactory<EntityCommandBufferTweenBuilder>(new EntityCommandBufferTweenBuilder(_world.Unmanaged, ecb));
            factory.CreateNullTween().WithDuration(1f).Play();
            ecb.Playback(_world.EntityManager);
            using var query = _world.EntityManager.CreateEntityQuery(typeof(TweenDuration), typeof(TweenTimer), typeof(TweenRequestPlaying));
            Assert.That(query.CalculateEntityCount(), Is.EqualTo(1));
        }
        [TestCase(0f)] [TestCase(.1f)]
        public void Transform_Completion_RemovesAllArrayAndMapEntries(float duration)
        {
            var objects = new[] { new GameObject(), new GameObject(), new GameObject() };
            try
            {
                foreach (var target in objects)
                    _factory.CreateTween().WithTransform(target.transform).WithLocalPositionOutput(new float3(1, 2, 3)).WithDuration(duration).Play();
                Tick(.25f); Drain();
                using var query = _world.EntityManager.CreateEntityQuery(typeof(TransformAccessSingleton));
                var access = query.GetSingleton<TransformAccessSingleton>();
                Assert.That(access.EntityLookup.Length, Is.Zero);
                Assert.That(access.TransformAccessArray.length, Is.Zero);
                Assert.That(access.IndexLookup.Count, Is.Zero);
                Tick(.25f); Tick(.25f);
                foreach (var target in objects) Assert.That(target.transform.position, Is.EqualTo(new Vector3(1, 2, 3)));
            }
            finally { foreach (var target in objects) Object.DestroyImmediate(target); }
        }
        [Test] public void Transform_DestroyedBeforePlay_SuppressesOutputAndSequenceContinues()
        {
            var target = new GameObject();
            var first = _factory.CreateTween().WithTransform(target.transform).WithLocalPositionOutput(new float3(1, 2, 3)).WithDuration(.1f);
            var next = _factory.CreateTween().WithDuration(1f);
            _factory.Serial("destroyed target").Append(first).Append(next).Build().Play();
            Object.DestroyImmediate(target);
            Tick(.25f); Drain();
            Assert.That(_world.EntityManager.GetComponentData<TweenTimer>(next.Entity).Value, Is.EqualTo(.15f).Within(.0001));
            using var query = _world.EntityManager.CreateEntityQuery(typeof(TransformAccessSingleton));
            Assert.That(query.GetSingleton<TransformAccessSingleton>().IndexLookup.Count, Is.Zero);
        }
        [Test] public void Conflict_Takeover_ReleasesOnlyOldOwnersOtherOutput()
        {
            var target = _world.EntityManager.CreateEntity(typeof(LocalTransform));
            _world.EntityManager.SetComponentData(target, LocalTransform.Identity);
            var a = _factory.CreateTween().WithTarget(target).WithLocalPositionOutput(new float3(1)).WithLocalRotationOutput(quaternion.RotateX(1)).WithDuration(1).Play();
            Tick(.1f);
            var b = _factory.CreateTween().WithTarget(target).WithLocalPositionOutput(new float3(2)).WithDuration(1).Play();
            Tick(.1f); Drain();
            using var query = _world.EntityManager.CreateEntityQuery(typeof(ConflictLookup));
            var lookup = query.GetSingleton<ConflictLookup>();
            Assert.That(lookup.EntityTypeKeyToTweenMap.TryGetValue(new EntityTypeKey(target, ComponentType.ReadOnly<TweenLocalPosition>()), out var owner), Is.True);
            Assert.That(owner, Is.EqualTo(b.Entity));
            Assert.That(lookup.EntityTypeKeyToTweenMap.ContainsKey(new EntityTypeKey(target, ComponentType.ReadOnly<TweenLocalRotation>())), Is.False);
            var c = _factory.CreateTween().WithTarget(target).WithLocalRotationOutput(quaternion.RotateY(1)).WithDuration(.1f).Play();
            Tick(.2f); Drain(); Tick(1); Drain();
            Assert.That(query.GetSingleton<ConflictLookup>().EntityTypeKeyToTweenMap.Count, Is.Zero);
        }
    }
}
#endif
