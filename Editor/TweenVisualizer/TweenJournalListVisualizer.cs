#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Systems;
using Unity.Collections;
using Unity.Entities;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BreezeBlockGames.HyperTween.Editor.TweenVisualizer
{
    [UxmlElement]
    public partial class TweenJournalListVisualizer : TweenJournalVisualizer
    {
        private struct EntryWithNames
        {
            public TweenJournal.Entry JournalEntry;
            public string EntityName;
            public string TargetEntityName;
        }
        
        private readonly List<EntryWithNames> _filteredEntries = new();
        private readonly List<EntryWithNames> _allEntries = new();
        
        private MultiColumnListView _multiColumnListView = null!;
        private DataBinding _dataBinding = null!;
        private EnumFlagsField _eventTypeFilter = null!;

        protected override void OnAttachToPanel()
        {
            _multiColumnListView = this.Q<MultiColumnListView>();
            
            _multiColumnListView.columns.Add(new Column()
            {
                title = "Journal Index",
                stretchable = true,
                bindCell = (element, i) =>
                {
                    element.SetBinding("text", new DataBinding()
                    {
                        dataSource = _filteredEntries[i].JournalEntry.Index
                    });
                },
                comparison = (a, b) => _filteredEntries[a].JournalEntry.Index.CompareTo(_filteredEntries[b].JournalEntry.Index)
            });
            
            _multiColumnListView.columns.Add(new Column()
            {
                title = "Frame",
                stretchable = true,
                bindCell = (element, i) =>
                {
                    element.SetBinding("text", new DataBinding()
                    {
                        dataSource = _filteredEntries[i].JournalEntry.Frame
                    });
                },
                comparison = (a, b) => _filteredEntries[a].JournalEntry.Frame.CompareTo(_filteredEntries[b].JournalEntry.Frame)
            });
            
            _multiColumnListView.columns.Add(new Column()
            {
                title = "Time",
                stretchable = true,
                bindCell = (element, i) =>
                {
                    element.SetBinding("text", new DataBinding()
                    {
                        dataSource = _filteredEntries[i].JournalEntry.Time
                    });
                },
                comparison = (a, b) => _filteredEntries[a].JournalEntry.Time.CompareTo(_filteredEntries[b].JournalEntry.Time)
            });
            
            _multiColumnListView.columns.Add(new Column()
            {
                title = "Iteration",
                stretchable = true,
                bindCell = (element, i) =>
                {
                    element.SetBinding("text", new DataBinding()
                    {
                        dataSource = _filteredEntries[i].JournalEntry.Iteration
                    });
                },
                comparison = (a, b) => _filteredEntries[a].JournalEntry.Iteration.CompareTo(_filteredEntries[b].JournalEntry.Iteration)
            });

            _multiColumnListView.columns.Add(new Column()
            {
                title = "Event",
                stretchable = true,
                bindCell = (element, i) =>
                {
                    element.SetBinding("text", new DataBinding()
                    {
                        dataSource = _filteredEntries[i].JournalEntry.LiteEntry.Event
                    });
                },
                comparison = (a, b) => _filteredEntries[a].JournalEntry.LiteEntry.Event.CompareTo(_filteredEntries[b].JournalEntry.LiteEntry.Event)
            });
            
            _multiColumnListView.columns.Add(new Column()
            {
                title = "Value",
                stretchable = true,
                bindCell = (element, i) =>
                {
                    element.SetBinding("text", new DataBinding()
                    {
                        dataSource = _filteredEntries[i].JournalEntry.LiteEntry.Value
                    });
                },
                comparison = (a, b) => _filteredEntries[a].JournalEntry.LiteEntry.Value.CompareTo(_filteredEntries[b].JournalEntry.LiteEntry.Value)
            });
            
            _multiColumnListView.columns.Add(new Column()
            {
                title = "Entity",
                stretchable = true,
                bindCell = (element, i) =>
                {
                    element.SetBinding("text", new DataBinding()
                    {
                        dataSource = _filteredEntries[i].JournalEntry.LiteEntry.Entity
                    });
                },
                comparison = (a, b) => _filteredEntries[a].JournalEntry.LiteEntry.Entity.CompareTo(_filteredEntries[b].JournalEntry.LiteEntry.Entity)
            });
            
            _multiColumnListView.columns.Add(new Column()
            {
                title = "Entity Name",
                stretchable = true,
                bindCell = (element, i) =>
                {
                    element.SetBinding("text", new DataBinding()
                    {
                        dataSource = _filteredEntries[i].EntityName
                    });
                },
                comparison = (a, b) => _filteredEntries[a].JournalEntry.LiteEntry.Entity.CompareTo(_filteredEntries[b].JournalEntry.LiteEntry.Entity)
            });
            
            _multiColumnListView.columns.Add(new Column()
            {
                title = "Target Entity",
                stretchable = true,
                bindCell = (element, i) =>
                {
                    element.SetBinding("text", new DataBinding()
                    {
                        dataSource = _filteredEntries[i].JournalEntry.LiteEntry.TargetEntity
                    });
                },
                comparison = (a, b) => Comparer<Entity?>.Default.Compare(_filteredEntries[a].JournalEntry.LiteEntry.TargetEntity, _filteredEntries[b].JournalEntry.LiteEntry.TargetEntity)
            });
            
            _multiColumnListView.columns.Add(new Column()
            {
                title = "Target Entity Name",
                stretchable = true,
                bindCell = (element, i) =>
                {
                    element.SetBinding("text", new DataBinding()
                    {
                        dataSource = _filteredEntries[i].TargetEntityName
                    });
                },
                comparison = (a, b) => _filteredEntries[a].JournalEntry.LiteEntry.Entity.CompareTo(_filteredEntries[b].JournalEntry.LiteEntry.Entity)
            });

            _dataBinding = new DataBinding()
            {
                dataSource = _filteredEntries,
                bindingMode = BindingMode.ToTarget,
                updateTrigger = BindingUpdateTrigger.WhenDirty
            };
            
            _multiColumnListView.SetBinding("itemsSource", _dataBinding);
            _multiColumnListView.columns.stretchMode = Columns.StretchMode.GrowAndFill;

            _dataBinding.MarkDirty();

            _eventTypeFilter = this.Q<EnumFlagsField>("EventTypeFilter");
            _eventTypeFilter.RegisterValueChangedCallback(_ =>
            {
                _filteredEntries.Clear();
                _filteredEntries.AddRange(_allEntries.Where(entryWithNames => MatchesFilter(in entryWithNames)));
            });
        }

        private bool MatchesFilter(in EntryWithNames entry)
        {
            var eventTypeFilter = (TweenJournal.Event)_eventTypeFilter.value;
            var @event = entry.JournalEntry.LiteEntry.Event;
            return (@event & eventTypeFilter) == @event;
        }
        
        protected override void OnReadFromJournal(RefRW<TweenJournalSingleton> tweenJournalSingleton)
        {
            if (tweenJournalSingleton.ValueRW.Entries.Length == 0)
            {
                return;
            }
            
            foreach (var entry in tweenJournalSingleton.ValueRW.Entries)
            {
                if (!tweenJournalSingleton.ValueRW.NameLookup.TryGetValue(entry.LiteEntry.Entity, out var entityName))
                {
                    entityName = "Unknown";
                }

                FixedString64Bytes targetEntityName = string.Empty;
                if (!entry.LiteEntry.TargetEntity.Equals(Entity.Null))
                {
                    if (!tweenJournalSingleton.ValueRW.NameLookup.TryGetValue(entry.LiteEntry.TargetEntity, out targetEntityName))
                    {
                        targetEntityName = "Unknown";
                    }
                }

                var entryWithNames = new EntryWithNames()
                {
                    JournalEntry = entry,
                    EntityName = entityName.ToString(),
                    TargetEntityName = targetEntityName.ToString()
                };
                _allEntries.Add(entryWithNames);

                if (!MatchesFilter(in entryWithNames))
                {
                    continue;
                }
                
                _filteredEntries.Add(entryWithNames);
            }
            
            _dataBinding.MarkDirty();
        }

        public override void ClearAllTweens()
        {
            _allEntries.Clear();
            _filteredEntries.Clear();
            _dataBinding.MarkDirty();
        }

        public override void ClearDestroyedTweens()
        {
            var destroyedEntities = _allEntries
                .Where(entry => entry.JournalEntry.LiteEntry.Event == TweenJournal.Event.Destroy)
                .Select(entry => entry.JournalEntry.LiteEntry.Entity)
                .ToHashSet();

            for (var i = _allEntries.Count - 1; i >= 0; i--)
            {
                var entry = _allEntries[i];
                if (destroyedEntities.Contains(entry.JournalEntry.LiteEntry.Entity))
                {
                    _allEntries.RemoveAt(i);
                }
            }
            
            for (var i = _filteredEntries.Count - 1; i >= 0; i--)
            {
                var entry = _filteredEntries[i];
                if (destroyedEntities.Contains(entry.JournalEntry.LiteEntry.Entity))
                {
                    _filteredEntries.RemoveAt(i);
                }
            }
        }
    }
#endif
}