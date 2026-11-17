using System;
using Unity.Collections;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.Journal.Components
{
    public struct TweenJournalSingleton : IComponentData, IDisposable
    {
        public NativeList<TweenJournal.LiteEntry> LiteEntries;
        public NativeList<TweenJournal.Entry> Entries;
        public NativeHashMap<Entity, FixedString64Bytes> NameLookup;
        public NativeReference<int> Index;
        public NativeReference<int> Length;
        public NativeReference<int> Count;
        public NativeReference<int> CurrentFrame;
        public NativeReference<int> CurrentStructuralChangeIteration;
        public NativeReference<bool> Consumed;
        
        public void EnsureCapacity(int additionalEntryCount)
        {
            var requiredCapacity = LiteEntries.Length + additionalEntryCount;
            if (requiredCapacity > LiteEntries.Capacity)
            {
                LiteEntries.SetCapacity(requiredCapacity);
            }
        }
        
        public void Dispose()
        {
            LiteEntries.Dispose();
            Entries.Dispose();
            NameLookup.Dispose();
            Index.Dispose();
            Length.Dispose();
            Count.Dispose();
            CurrentFrame.Dispose();
            CurrentStructuralChangeIteration.Dispose();
            Consumed.Dispose();
        }

        public static void Create(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new TweenJournalSingleton()
            {
                LiteEntries = new NativeList<TweenJournal.LiteEntry>(512 , Allocator.Persistent),
                Entries = new NativeList<TweenJournal.Entry>(512 , Allocator.Persistent),
                NameLookup = new NativeHashMap<Entity, FixedString64Bytes>(512, Allocator.Persistent),
                Index = new NativeReference<int>(Allocator.Persistent),
                Length = new NativeReference<int>(Allocator.Persistent),
                Count = new NativeReference<int>(Allocator.Persistent),
                CurrentFrame = new NativeReference<int>(Allocator.Persistent),
                CurrentStructuralChangeIteration = new NativeReference<int>(Allocator.Persistent),
                Consumed = new NativeReference<bool>(Allocator.Persistent),
            });
        }
    }
}