#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using BreezeBlockGames.HyperTween.Editor.VisualElements;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Components;
using BreezeBlockGames.HyperTween.UnityShared.Journal.Systems;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BreezeBlockGames.HyperTween.Editor.TweenVisualizer
{
    [UxmlElement]
    public partial class TweenJournalGraphVisualizer : TweenJournalVisualizer
    {
        private const float NodeViewWidth = 200f;
        private const float NodeViewHeight = 100f;
        private const float LayoutSpacing = 50f;

        public struct NodeKey : IEquatable<NodeKey>
        {
            public Entity Entity;
            public TweenNodeView.Type Type;

            public override string ToString()
            {
                return $"{nameof(Entity)}: {Entity}, {nameof(Type)}: {Type}";
            }

            public bool Equals(NodeKey other)
            {
                return Entity.Equals(other.Entity) && Type == other.Type;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Entity, (int)Type);
            }
        }
        
        public struct EdgeKey : IEquatable<EdgeKey>
        {
            public NodeKey FromKey;
            public NodeKey ToKey;
            public TweenEdgeView.Type Type;

            public override string ToString()
            {
                return $"{nameof(FromKey)}: {FromKey}, {nameof(ToKey)}: {ToKey}, {nameof(Type)}: {Type}";
            }

            public bool Equals(EdgeKey other)
            {
                return FromKey.Equals(other.FromKey) && ToKey.Equals(other.ToKey) && Type == other.Type;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(FromKey, ToKey, (int)Type);
            }
        }

        private readonly Dictionary<NodeKey, TweenNodeView> _nodes = new();
        private readonly Dictionary<EdgeKey, TweenEdgeView> _edges = new();
        private readonly VisualTreeAsset _nodeViewAsset;
        private readonly VisualTreeAsset _edgeViewAsset;
        
        private List<HashSet<NodeKey>> _islands = new();
        private bool _needsLayout = true;

        private VisualElements.ZoomView _zoomView = null!;

        public TweenJournalGraphVisualizer()
        {
            const string uxmlPath = "Packages/com.breezeblockgames.hyper-tween/Editor/uxml/GraphVisualizer";
            
            _nodeViewAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{uxmlPath}/TweenNodeView.uxml");
            _edgeViewAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{uxmlPath}/TweenEdgeView.uxml");
        }
        
        protected override void OnAttachToPanel()
        {
            _zoomView = this.Q<ZoomView>();
            _zoomView.StretchToParentSize();
        }
        
        private void UpsertNode(NodeKey key, string nodeName, TweenNodeView.State state, bool create = true)
        {
            if (!_nodes.TryGetValue(key, out var node))
            {
                if (!create)
                {
                    return;
                }
            
                node = _nodeViewAsset.Instantiate().Q<TweenNodeView>();
                node.Key = key;
                node.CurrentData.Name = nodeName;
                node.CurrentData.Entity = key.Entity;
                node.CurrentData.Type = key.Type;
                
                _zoomView.Add(node);
            
                _nodes.Add(key, node);
                _needsLayout = true;
            }

            node.CurrentData.State = state;
        }

        private void UpsertEdge(EdgeKey edgeKey, TweenEdgeView.State edgeState, float? progress = default, bool create = true)
        {
            if (!_nodes.TryGetValue(edgeKey.FromKey, out var fromNode))
            {
                Debug.LogError($"edgeKey.FromKey not found: {edgeKey.FromKey}");
                return;
            }
        
            if (!_nodes.TryGetValue(edgeKey.ToKey, out var toNode))
            {
                Debug.LogError($"edgeKey.ToKey not found: {edgeKey.ToKey}");
                return;
            }

            if (!_edges.TryGetValue(edgeKey, out var edge))
            {
                if (!create)
                {
                    return;
                }

                edge = _edgeViewAsset.Instantiate().Q<TweenEdgeView>();
                edge.Key = edgeKey;
                edge.CurrentData.Type = edgeKey.Type;
                edge.FromNodeView = fromNode;
                edge.ToNodeView = toNode;
                
                fromNode.AddOutgoingEdge(edge);
                toNode.AddIncomingEdge(edge);
                
                _zoomView.Add(edge);
            
                _edges.Add(edgeKey, edge);
                _needsLayout = true;
            }

            edge.CurrentData.State = edgeState;
            if (progress.HasValue)
            {
                edge.CurrentData.Progress = progress.Value;
            }
        }

        protected override void OnReadFromJournal(RefRW<TweenJournalSingleton> tweenJournalSingleton)
        {
            foreach (var entry in tweenJournalSingleton.ValueRW.Entries)
            {
                var playNodeKey = new NodeKey()
                {
                    Entity = entry.LiteEntry.Entity,
                    Type = TweenNodeView.Type.PlayNode
                };

                var stopNodeKey = new NodeKey()
                {
                    Entity = entry.LiteEntry.Entity,
                    Type = TweenNodeView.Type.StopNode
                };

                tweenJournalSingleton.ValueRW.NameLookup.TryGetValue(entry.LiteEntry.Entity, out var entityFixedName);
                var entityName = entityFixedName.ToString();
                
                switch (entry.LiteEntry.Event)
                {
                    case TweenJournal.Event.Create:
                        UpsertNode(playNodeKey, entityName, TweenNodeView.State.Created);
                        UpsertNode(stopNodeKey, entityName, TweenNodeView.State.Created);
                        break;
                    case TweenJournal.Event.OnPlay:
                        UpsertNode(playNodeKey, entityName, TweenNodeView.State.Played);
                        break;
                    case TweenJournal.Event.OnStop:
                        UpsertNode(stopNodeKey, entityName, TweenNodeView.State.Stopped);
                        break;
                    case TweenJournal.Event.Conflict:
                        UpsertEdge(new EdgeKey()
                        {
                            FromKey = playNodeKey,
                            ToKey = stopNodeKey,
                            Type = TweenEdgeView.Type.Duration
                        }, TweenEdgeView.State.Conflicted, 1f);
                        break;
                    case TweenJournal.Event.UpdatedTimer:
                        UpsertEdge(new EdgeKey()
                        {
                            FromKey = playNodeKey,
                            ToKey = stopNodeKey,
                            Type = TweenEdgeView.Type.Duration
                        }, TweenEdgeView.State.Created);
                        break;
                    case TweenJournal.Event.UpdatedParameter:
                        UpsertEdge(new EdgeKey()
                        {
                            FromKey = playNodeKey,
                            ToKey = stopNodeKey,
                            Type = TweenEdgeView.Type.Duration
                        }, TweenEdgeView.State.Created, entry.LiteEntry.Value);
                        break;
                    case TweenJournal.Event.Output:
                        break;
                    case TweenJournal.Event.CreateDuration:
                        UpsertEdge(new EdgeKey()
                        {
                            FromKey = playNodeKey,
                            ToKey = stopNodeKey,
                            Type = TweenEdgeView.Type.Duration
                        }, TweenEdgeView.State.Created);
                        break;
                    case TweenJournal.Event.CompleteDuration:
                        UpsertEdge(new EdgeKey()
                        {
                            FromKey = playNodeKey,
                            ToKey = stopNodeKey,
                            Type = TweenEdgeView.Type.Duration
                        }, TweenEdgeView.State.Completed, 1f);
                        break;
                    case TweenJournal.Event.CreateForkOnPlay:
                        UpsertEdge(entry, TweenNodeView.Type.PlayNode, TweenNodeView.Type.PlayNode, TweenEdgeView.Type.ForkOnPlay, TweenEdgeView.State.Created, 0f);
                        break;
                    case TweenJournal.Event.CompleteForkOnPlay:
                        UpsertEdge(entry, TweenNodeView.Type.PlayNode, TweenNodeView.Type.PlayNode, TweenEdgeView.Type.ForkOnPlay, TweenEdgeView.State.Completed, 1f);
                        break;
                    case TweenJournal.Event.CreateSignalJoinOnStop:
                        UpsertEdge(entry, TweenNodeView.Type.StopNode, TweenNodeView.Type.StopNode, TweenEdgeView.Type.SignalJoinOnStop, TweenEdgeView.State.Created, 0f);
                        break;
                    case TweenJournal.Event.CompleteSignalJoinOnStop:
                        UpsertEdge(entry, TweenNodeView.Type.StopNode, TweenNodeView.Type.StopNode, TweenEdgeView.Type.SignalJoinOnStop, TweenEdgeView.State.Completed, 1f);
                        break;
                    case TweenJournal.Event.CreatePlayOnPlay:
                        UpsertEdge(entry, TweenNodeView.Type.PlayNode, TweenNodeView.Type.PlayNode, TweenEdgeView.Type.PlayOnPlay, TweenEdgeView.State.Created, 0f);
                        break;
                    case TweenJournal.Event.CompletePlayOnPlay:
                        UpsertEdge(entry, TweenNodeView.Type.PlayNode, TweenNodeView.Type.PlayNode, TweenEdgeView.Type.PlayOnPlay, TweenEdgeView.State.Completed, 1f);
                        break;
                    case TweenJournal.Event.CreatePlayOnStop:
                        UpsertEdge(entry, TweenNodeView.Type.StopNode, TweenNodeView.Type.PlayNode, TweenEdgeView.Type.PlayOnStop, TweenEdgeView.State.Created, 0f);
                        break;
                    case TweenJournal.Event.CompletePlayOnStop:
                        UpsertEdge(entry, TweenNodeView.Type.StopNode, TweenNodeView.Type.PlayNode, TweenEdgeView.Type.PlayOnStop, TweenEdgeView.State.Completed, 1f);
                        break;
                    case TweenJournal.Event.Destroy:
                        UpsertNode(playNodeKey, entityName, TweenNodeView.State.Destroyed);
                        UpsertNode(stopNodeKey, entityName, TweenNodeView.State.Destroyed);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (_needsLayout)
            {
                RecalculateLayout();
            }
        }

        private void UpsertEdge(TweenJournal.Entry entry, TweenNodeView.Type fromNodeType, TweenNodeView.Type toNodeType, TweenEdgeView.Type edgeType, TweenEdgeView.State edgeState, float? progress)
        {
            if (entry.LiteEntry.TargetEntity.Equals(Entity.Null))
            {
                Debug.LogError("Edge entry must have target entity");
                return;
            }
                        
            var edgeKey = new EdgeKey()
            {
                FromKey = new NodeKey()
                {
                    Entity = entry.LiteEntry.Entity,
                    Type = fromNodeType
                },
                ToKey = new NodeKey()
                {
                    Entity = entry.LiteEntry.TargetEntity,
                    Type = toNodeType
                },
                Type = edgeType
            };
                        
            UpsertEdge(edgeKey, edgeState, progress);
        }

        public override void ClearAllTweens()
        {
            foreach (var node in _nodes.Values)
            {
                _zoomView.Remove(node);
            }

            foreach (var edgeView in _edges.Values)
            {
                _zoomView.Remove(edgeView);
            }
            
            _nodes.Clear();
            _edges.Clear();
        }

        public override void ClearDestroyedTweens()
        {
            var destroyedIslands = _islands
                .Where(set => set.All(key => !_nodes.TryGetValue(key, out var view) || view.CurrentData.State == TweenNodeView.State.Destroyed));

            var destroyedNodeViews = destroyedIslands.SelectMany(set => set
                    .Select(key => (_nodes.TryGetValue(key, out var view), view))
                    .Where(tuple => tuple.Item1)
                    .Select(tuple => tuple.view))
                .ToArray();

            var destroyedEdges = destroyedNodeViews
                .SelectMany(view => view?.IncomingEdges.Concat(view.OutgoingEdges))
                .ToHashSet();

            foreach (var view in destroyedNodeViews)
            {
                if (view == null)
                {
                    continue;
                }
                
                _zoomView.Remove(view);
                _nodes.Remove(view.Key);
            }
            
            foreach (var edge in destroyedEdges)
            {
                _zoomView.Remove(edge);
                _edges.Remove(edge.Key);
            }

            RecalculateLayout();
        }

        private void RecalculateLayout()
        {
            LayoutNodesSugiyama();
            _needsLayout = false;
        }
        
        private void LayoutNodesSugiyama()
        {
            PopulateIslands();
            
            float yOffset = 0;
            const float islandPadding = NodeViewWidth;

            foreach (var island in _islands)
            {
                var layers = AssignLayers(island);
                var positions = ComputePositions(layers);

                // Compute island bounds to calculate offsets
                var bounds = ComputeIslandBounds(positions);
                var offset = new Vector2(-bounds.xMin, yOffset - bounds.yMin);

                // Apply offset positions to nodes
                foreach (var kvp in positions)
                {
                    _nodes[kvp.Key].Position = kvp.Value + offset;
                }

                foreach (var node in _nodes.Values)
                {
                    node.SortEdges();
                }

                // Stack islands vertically, adjusting yOffset downward
                yOffset += bounds.height + islandPadding;
            }
        }


        // 1. Identify connected components (islands)
        private void PopulateIslands()
        {
            var visited = new HashSet<NodeKey>();

            _islands = new();
            foreach (var node in _nodes.Keys)
            {
                if (!visited.Contains(node))
                {
                    var island = new HashSet<NodeKey>();
                    DepthFirstSearchIslands(node, island, visited);
                    _islands.Add(island);
                }
            }
        }

        private void DepthFirstSearchIslands(NodeKey node, HashSet<NodeKey> island, HashSet<NodeKey> visited)
        {
            visited.Add(node);
            island.Add(node);

            foreach (var edge in _edges.Keys)
            {
                if (edge.FromKey.Equals(node) && !visited.Contains(edge.ToKey))
                {
                    DepthFirstSearchIslands(edge.ToKey, island, visited);
                }
                
                if (edge.ToKey.Equals(node) && !visited.Contains(edge.FromKey))
                {
                    DepthFirstSearchIslands(edge.FromKey, island, visited);
                }
            }
        }

        // Sugiyama layering algorithm (BFS approach for simplicity)
        private Dictionary<int, List<NodeKey>> AssignLayers(HashSet<NodeKey> island)
        {
            var layers = new Dictionary<int, List<NodeKey>>();
            var inDegree = new Dictionary<NodeKey, int>();

            foreach (var node in island)
            {
                inDegree[node] = 0;
            }

            foreach (var edge in _edges.Keys)
            {
                if (island.Contains(edge.ToKey))
                {
                    inDegree[edge.ToKey]++;
                }
            }

            var queue = new Queue<(NodeKey node, int layer)>();

            // Start from nodes with zero in-degree
            foreach (var node in island)
            {
                if (inDegree[node] == 0)
                {
                    queue.Enqueue((node, 0));
                }
            }

            var visited = new HashSet<NodeKey>();

            while (queue.Count > 0)
            {
                var (node, layer) = queue.Dequeue();
                if (!layers.ContainsKey(layer))
                {
                    layers[layer] = new List<NodeKey>();
                }

                layers[layer].Add(node);
                visited.Add(node);

                foreach (var edge in _edges.Keys.Where(e => e.FromKey.Equals(node)))
                {
                    if (!island.Contains(edge.ToKey))
                    {
                        continue;
                    }

                    inDegree[edge.ToKey]--;
                    if (inDegree[edge.ToKey] == 0)
                    {
                        queue.Enqueue((edge.ToKey, layer + 1));
                    }
                }
            }

            // Any nodes still not visited (cycles not allowed in DAG, but safeguard)
            foreach (var node in island.Except(visited))
            {
                if (!layers.ContainsKey(0))
                {
                    layers[0] = new List<NodeKey>();
                }
                
                layers[0].Add(node);
            }

            return layers;
        }

        // Compute node positions within layers (columns are layers, vertically spread nodes)
        private static Dictionary<NodeKey, Vector2> ComputePositions(Dictionary<int, List<NodeKey>> layers)
        {
            const float xSpacing = NodeViewWidth + LayoutSpacing;
            const float ySpacing = NodeViewHeight + LayoutSpacing;

            var positions = new Dictionary<NodeKey, Vector2>();

            foreach (var layer in layers)
            {
                var layerIndex = layer.Key;
                var nodesInLayer = layer.Value;
                var layerHeight = (nodesInLayer.Count - 1) * ySpacing;
                var yStart = -layerHeight / 2f;

                for (var i = 0; i < nodesInLayer.Count; i++)
                {
                    var x = layerIndex * xSpacing;
                    var y = yStart + i * ySpacing;
                    positions[nodesInLayer[i]] = new Vector2(x, y);
                }
            }

            return positions;
        }

        private Rect ComputeIslandBounds(Dictionary<NodeKey, Vector2> positions)
        {
            var min = Vector2.positiveInfinity;
            var max = Vector2.negativeInfinity;
            
            foreach (var kvp in positions)
            {
                min = Vector2.Min(kvp.Value, min);    
                max = Vector2.Max(kvp.Value, max);    
            }
            
            return Rect.MinMaxRect(min.x, min.y, max.x + NodeViewWidth, max.y + NodeViewHeight);
        }
    }

#endif
}