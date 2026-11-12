using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

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

        private struct EntryComparer : IComparer<Entry>
        {
            public int Compare(Entry x, Entry y)
            {
                return x.SortOrder.CompareTo(y.SortOrder);
            }
        }

        private readonly UnsafeList<Entry>* _entries;
        private readonly UnsafeList<int>* _delimiters;
        private readonly UnsafeList<bool>* _withJournalingStack;
        
        private TweenCollection(UnsafeList<Entry>* entries, UnsafeList<int>* delimiters, UnsafeList<bool>* withJournalingStack)
        {
            _withJournalingStack = withJournalingStack;
            _entries = entries;
            _delimiters = delimiters;
        }
        
        public static TweenCollection Create(bool withJournaling = HyperTweenFactory.JournalingByDefault) 
        {
            var entries = UnsafeList<Entry>.Create(8, Allocator.Persistent);
            var delimiters = UnsafeList<int>.Create(8, Allocator.Persistent);
            var withJournalingStack = UnsafeList<bool>.Create(8, Allocator.Persistent);

            withJournalingStack->Add(withJournaling);
            
            return new TweenCollection(entries, delimiters, withJournalingStack);
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
            _delimiters->Add(_entries->Length);
        }

        /// <summary>
        /// Removes and returns the most recently pushed group of tween entries as a composite tween handle.
        /// Popped entries are organized into a serial sequence where entries with the same sort order 
        /// execute in parallel, and different sort orders execute serially in ascending order.
        /// </summary>
        public TweenHandle Pop(in FixedString64Bytes name)
        {
            var factory = HyperTweenFactory.Get(GetWithJournaling());
            var lastIndex = _delimiters->Length - 1;
            var delimiter = _delimiters->ElementAt(lastIndex);
            _delimiters->RemoveAt(lastIndex);

            var numEntries = _entries->Length - delimiter;
            
            // We can just early-out here with a null tween handle
            if (numEntries == 0)
            {
                return factory.CreateNullTween();
            }
            
            var poppedEntries = new NativeArray<Entry>(numEntries, Allocator.Temp);
            try
            {
                for (int i = delimiter; i < _entries->Length; i++)
                {
                    poppedEntries[i - delimiter] = _entries->ElementAt(i);
                }
                _entries->RemoveRange(delimiter, numEntries);
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

        public void AddTween(int sortOrder, TweenHandle tweenHandle)
        {
            _entries->Add(new Entry
            {
                SortOrder = sortOrder, TweenHandle = tweenHandle
            });
        }

        public void Dispose()
        {
            _entries->Dispose();
            _delimiters->Dispose();
            _withJournalingStack->Dispose();
        }
    }
}