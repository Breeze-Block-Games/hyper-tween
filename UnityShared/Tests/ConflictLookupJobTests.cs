#if UNITY_INCLUDE_TESTS
using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.ConflictDetection.Systems;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Update.Components;
using BreezeBlockGames.HyperTween.UnityShared.ECS.Util;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.Tests
{
    public class ConflictLookupJobTests
    {
        public struct Output : IComponentData { public float Value; }
        public struct Id32 : IComponentData { public int Value; }
        public struct Id64 : IComponentData { public ulong Value; }
        public struct Metadata { public TweenComponentMetaData Output; }
        [Test] public void ConflictJobs_PreserveDistinct64BitOwnersAndTakeoverDuringCleanup()
        { Exercise(new Id64 { Value = 0x100000001UL }, new Id64 { Value = 0x200000001UL }, 0x100000001UL, 0x200000001UL); }
        [Test] public void ConflictJobs_PreserveNegative32BitOwnersAndTakeoverDuringCleanup()
        { Exercise(new Id32 { Value = -1 }, new Id32 { Value = 1 }, uint.MaxValue, 1); }

        private static void Exercise<TId>(TId firstId, TId secondId, ulong firstKey, ulong secondKey) where TId : unmanaged, IComponentData
        {
            using var world = new World(nameof(ConflictLookupJobTests));
            var em = world.EntityManager;
            using var instances = new NativeHashMap<InstanceIdKey, Entity>(8, Allocator.TempJob);
            using var entities = new NativeHashMap<EntityTypeKey, Entity>(8, Allocator.TempJob);
            using var fields = new FieldEnumerable<Metadata, TweenComponentMetaData>(Allocator.TempJob);
            Entity Create(TId id)
            {
                var e = em.CreateEntity(typeof(Output), typeof(TId));
                em.SetComponentData(e, id);
                return e;
            }
            Metadata Meta() => new Metadata { Output = new TweenComponentMetaData {
                TargetComponentTypeInfo = new DynamicTypeInfo { ComponentType = ComponentType.ReadOnly<Output>(), DynamicComponentTypeHandle = em.GetDynamicComponentTypeHandle(ComponentType.ReadOnly<Output>()) },
                InstanceIdComponentTypeInfo = new DynamicTypeInfo { ComponentType = ComponentType.ReadOnly<TId>(), DynamicComponentTypeHandle = em.GetDynamicComponentTypeHandle(ComponentType.ReadOnly<TId>()) },
                HasInstanceIdComponent = true
            } };
            void Add(Entity e)
            {
                using var ecb = new EntityCommandBuffer(Allocator.TempJob);
                var job = new AddToConflictLookupSystemJob<Metadata> {
                    EntityTypeKeyToTweenMap = entities, InstanceIdToTweenMap = instances,
                    MetaDataContainer = Meta(), TweenComponentsMetaDataEnumerable = fields,
                    EntityTypeHandle = em.GetEntityTypeHandle(), TweenTargetTypeHandle = em.GetComponentTypeHandle<TweenTarget>(true), EntityCommandBuffer = ecb
                };
                job.Execute(em.GetChunk(e), 0, false, default);
                ecb.Playback(em);
            }
            var first = Create(firstId); Add(first);
            var second = Create(secondId);
            // Separate chunks prevent re-adding the first entity when executing the real chunk job.
            em.AddComponent<TweenParameter>(second);
            Add(second);
            Assert.That(instances[new InstanceIdKey(firstKey, ComponentType.ReadOnly<Output>())], Is.EqualTo(first));
            Assert.That(instances[new InstanceIdKey(secondKey, ComponentType.ReadOnly<Output>())], Is.EqualTo(second));
            var replacement = Create(firstId);
            em.AddComponent<TweenTimer>(replacement);
            Add(replacement);
            Assert.That(em.HasComponent<TweenConflicted>(first), Is.True);
            var remove = new RemoveFromConflictLookupJob<Metadata> {
                TargetToTweenMap = entities, GameObjectTypeKeyToTweenMap = instances,
                MetaDataContainer = Meta(), TweenComponentsMetaDataEnumerable = fields,
                EntityTypeHandle = em.GetEntityTypeHandle(), TweenTargetTypeHandle = em.GetComponentTypeHandle<TweenTarget>(true)
            };
            remove.Execute(em.GetChunk(first), 0, false, default);
            Assert.That(instances.TryGetValue(new InstanceIdKey(firstKey, ComponentType.ReadOnly<Output>()), out var owner), Is.True);
            Assert.That(owner, Is.EqualTo(replacement));
            Assert.That(instances[new InstanceIdKey(secondKey, ComponentType.ReadOnly<Output>())], Is.EqualTo(second));
        }
    }
}
#endif
