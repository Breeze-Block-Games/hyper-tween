using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace BreezeBlockGames.HyperTween.UnityShared.API
{
    public readonly unsafe struct TweenCollection : IDisposable
    {
        public readonly struct JournalingScope : IDisposable
        {
            private readonly UnsafeList<bool>* _withJournalingStack;

            public JournalingScope(UnsafeList<bool>* withJournalingStack, bool withJournaling)
            {
                _withJournalingStack = withJournalingStack;
                _withJournalingStack->Add(withJournaling);
            }

            public void Dispose()
            {
                var lastIndex = _withJournalingStack->Length - 1;
                _withJournalingStack->RemoveAt(lastIndex);
            }
        }

        private struct Entry
        {
            public int SortOrder;
            public TweenHandle TweenHandle;
        }

        private struct Delimiter
        {
            public int EntryIndex, CreatedIndex;
        }
        private struct CreatedTween
        {
            public TweenHandle Handle; public bool Destroyed;
        }

        private struct EntryComparer : IComparer<Entry>
        {
            public int Compare(Entry x, Entry y)
            {
                return x.SortOrder.CompareTo(y.SortOrder);
            }
        }

        private readonly UnsafeList<Entry>* _entries;
        private readonly UnsafeList<Delimiter>* _delimiters;
        private readonly UnsafeList<bool>* _withJournalingStack;
        private readonly UnsafeList<CreatedTween>* _createdTweens;

        private TweenCollection(UnsafeList<Entry>* entries, UnsafeList<Delimiter>* delimiters, UnsafeList<bool>* withJournalingStack, UnsafeList<CreatedTween>* createdTweens)
        {
            _withJournalingStack = withJournalingStack;
            _entries = entries;
            _delimiters = delimiters;
            _createdTweens = createdTweens;
        }

        public static TweenCollection Create(bool withJournaling = HyperTweenFactory.JournalingByDefault)
        {
            var entries = UnsafeList<Entry>.Create(8, Allocator.Persistent);
            var delimiters = UnsafeList<Delimiter>.Create(8, Allocator.Persistent);
            var createdTweens = UnsafeList<CreatedTween>.Create(8, Allocator.Persistent);
            var withJournalingStack = UnsafeList<bool>.Create(8, Allocator.Persistent);

            withJournalingStack->Add(withJournaling);

            return new TweenCollection(entries, delimiters, withJournalingStack, createdTweens);
        }

        public TweenFactory<CollectionTweenBuilder> CreateTweenFactory()
        {
            var factory = HyperTweenFactory.Get();
            var tweenBuilder = new CollectionTweenBuilder(factory.TweenBuilder, this);
            return new TweenFactory<CollectionTweenBuilder>(tweenBuilder, GetWithJournaling());
        }

        public JournalingScope WithJournalingScope(bool withJournaling)
        {
#if !HYPER_TWEEN_ENABLE_JOURNAL
            if (withJournaling)
            {
                throw new InvalidOperationException("HYPER_TWEEN_ENABLE_JOURNAL is not defined");
            }
#endif

            return new JournalingScope(_withJournalingStack, withJournaling);
        }

        private bool GetWithJournaling()
        {
            var lastIndex = _withJournalingStack->Length - 1;
            return _withJournalingStack->ElementAt(lastIndex);
        }

        public void Push()
        {
            _delimiters->Add(new Delimiter { EntryIndex = _entries->Length, CreatedIndex = _createdTweens->Length });
        }

        /// <summary>
        /// Removes and returns the most recently pushed group of tween entries as a composite tween handle.
        /// Popped entries are organized into a serial sequence where entries with the same sort order 
        /// execute in parallel, and different sort orders execute serially in ascending order.
        /// </summary>
        public TweenHandle Pop(in FixedString64Bytes name)
        {
            var delimiter = TakeDelimiter();
            var factory = CreateTweenFactory();
            if (_delimiters->Length == 0)
                _createdTweens->RemoveRange(delimiter.CreatedIndex, _createdTweens->Length - delimiter.CreatedIndex);
            var numEntries = _entries->Length - delimiter.EntryIndex;

            // We can just early-out here with a null tween handle
            if (numEntries == 0)
            {
                return factory.CreateNullTween();
            }

            var poppedEntries = new NativeArray<Entry>(numEntries, Allocator.Temp);
            try
            {
                for (int i = delimiter.EntryIndex; i < _entries->Length; i++)
                {
                    poppedEntries[i - delimiter.EntryIndex] = _entries->ElementAt(i);
                }
                _entries->RemoveRange(delimiter.EntryIndex, numEntries);
                poppedEntries.Sort(new EntryComparer());

                var index = default(int?);
                var groupStart = 0;
                var serialBuilder = factory.Serial(in name);

                void AddParallelGroup(int fromIndex, int toIndex, in FixedString64Bytes name)
                {
                    var parallelBuilder = factory.Parallel(in name);
                    for (var j = fromIndex; j < toIndex; j++)
                    {
                        parallelBuilder.Append(poppedEntries[j].TweenHandle);
                    }
                    serialBuilder.Append(parallelBuilder.Build());
                }

                for (var i = 0; i < poppedEntries.Length; i++)
                {
                    var entry = poppedEntries[i];
                    if (!index.HasValue)
                    {
                        index = entry.SortOrder;
                    }

                    if (entry.SortOrder == index)
                    {
                        continue;
                    }
                    index = entry.SortOrder;

                    AddParallelGroup(groupStart, i, in name);
                    groupStart = i;
                }

                AddParallelGroup(groupStart, poppedEntries.Length, in name);

                return serialBuilder.Build();
            }
            finally
            {
                poppedEntries.Dispose();
            }
        }

        private Delimiter TakeDelimiter()
        {
            if (_delimiters->Length == 0)
                throw new InvalidOperationException("No tween collection group is open.");
            var index = _delimiters->Length - 1;
            var delimiter = _delimiters->ElementAt(index);
            _delimiters->RemoveAt(index);
            return delimiter;
        }

        internal void TrackCreatedEntity(Entity entity, WorldUnmanaged world)
        {
            if (_delimiters->Length != 0)
                _createdTweens->Add(new CreatedTween { Handle = new TweenHandle(entity, world) });
        }

        internal void MarkDestroyedEntity(Entity entity, WorldUnmanaged world)
        {
            for (var i = 0; i < _createdTweens->Length; i++)
            {
                ref var created = ref _createdTweens->ElementAt(i);
                if (created.Handle.Entity == entity && created.Handle.World.Equals(world))
                    created.Destroyed = true;
            }
        }

        /// <summary>Discard the current unplayed group and its created entities, preserving its enclosing group.</summary>
        public void Discard()
        {
            var delimiter = TakeDelimiter();
            var seen = new HashSet<(Entity, WorldUnmanaged)>();
            var destroy = new List<TweenHandle>();
            for (var i = 0; i < _createdTweens->Length; i++)
            {
                var created = _createdTweens->ElementAt(i);
                if (created.Destroyed && !created.Handle.IsNull)
                    seen.Add((created.Handle.Entity, created.Handle.World));
            }
            void Add(TweenHandle handle)
            {
                if (!handle.IsNull && seen.Add((handle.Entity, handle.World)))
                    destroy.Add(handle);
            }
            for (var i = _createdTweens->Length - 1; i >= delimiter.CreatedIndex; i--)
                Add(_createdTweens->ElementAt(i).Handle);
            for (var i = _entries->Length - 1; i >= delimiter.EntryIndex; i--)
                Add(_entries->ElementAt(i).TweenHandle);
            _createdTweens->RemoveRange(delimiter.CreatedIndex, _createdTweens->Length - delimiter.CreatedIndex);
            _entries->RemoveRange(delimiter.EntryIndex, _entries->Length - delimiter.EntryIndex);

            List<Exception>? errors = null;
            foreach (var handle in destroy)
            {
                try
                {
                    handle.GetBuilder().TweenBuilder.DestroyEntity(handle.Entity);
                }
                catch (Exception error) { (errors ??= new List<Exception>()).Add(error); }
            }
            if (errors != null)
                throw new AggregateException(errors);
        }

        public void AddTween(int sortOrder, TweenHandle tweenHandle)
        {
            _entries->Add(new Entry
            {
                SortOrder = sortOrder,
                TweenHandle = tweenHandle
            });
        }

        public void Dispose()
        {
            UnsafeList<Entry>.Destroy(_entries);
            UnsafeList<Delimiter>.Destroy(_delimiters);
            UnsafeList<bool>.Destroy(_withJournalingStack);
            UnsafeList<CreatedTween>.Destroy(_createdTweens);
        }
    }
}