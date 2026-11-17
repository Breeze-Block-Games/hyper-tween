#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using BreezeBlockGames.HyperTween.Editor.TweenVisualizer;
using Unity.Entities;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace BreezeBlockGames.HyperTween.Editor.VisualElements
{
    [UxmlElement]
    public partial class TweenNodeView : VisualElement
    {
        public class Data
        {
            [CreateProperty]
            public string? Name { get; set; }
            [CreateProperty]
            public Entity Entity { get; set; }
            [CreateProperty]
            public State State { get; set; }
            [CreateProperty]
            public Type Type { get; set; }
        }
        
        public enum State
        {
            None,
            Created,
            Played,
            Stopped,
            Destroyed
        }
        
        public enum Type
        {
            None,
            PlayNode,
            StopNode
        }
        
        private readonly List<TweenEdgeView> _incomingEdges = new();
        private readonly List<TweenEdgeView> _outgoingEdges = new();

        private VisualElement _visualElement = null!;

        public Data CurrentData { get; } = new();
        public TweenJournalGraphVisualizer.NodeKey Key { get; set; }
        
        public IEnumerable<TweenEdgeView> IncomingEdges => _incomingEdges;
        public IEnumerable<TweenEdgeView> OutgoingEdges => _outgoingEdges;

        private State _currentState;
        [CreateProperty]
        private State CurrentState
        {
            get => _currentState;
            set
            {
                _visualElement.RemoveFromClassList(GetStateClassName(_currentState));
                _currentState = value;
                _visualElement.AddToClassList(GetStateClassName(_currentState));
            }
        }

        private Type _currentType;
        [CreateProperty]
        private Type CurrentType
        {
            get => _currentType;
            set
            {
                _visualElement.RemoveFromClassList(GetTypeClassName(_currentType));
                _currentType = value;
                _visualElement.AddToClassList(GetTypeClassName(_currentType));
            }
        }

        public Vector2 Position
        {
            set
            {
                style.left = value.x;
                style.top = value.y;
            }
        }

        private string GetStateClassName(State state)
        {
            return state.ToString().ToLowerInvariant();
        }
        
        private static string GetTypeClassName(Type type)
        {
            return type switch
            {
                Type.None => "",
                Type.PlayNode => "tween-play-node",
                Type.StopNode => "tween-stop-node",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        
        public TweenNodeView()
        {
            dataSource = CurrentData;

            RegisterCallbackOnce<AttachToPanelEvent>(_ =>
            {
                _visualElement = this.Q<VisualElement>("Background");
                var nameTextElement = this.Q<TextElement>("Name");
                var entityTextElement = this.Q<TextElement>("Entity");

                SetBinding(nameof(CurrentState), new DataBinding()
                {
                    dataSourcePath = new PropertyPath(nameof(Data.State)),
                    dataSourceType = typeof(State)
                });
                
                SetBinding(nameof(CurrentType), new DataBinding()
                {
                    dataSourcePath = new PropertyPath(nameof(Data.Type)),
                    dataSourceType = typeof(Type)
                });
                
                nameTextElement.SetBinding("text", new DataBinding()
                {
                    dataSourcePath = PropertyPath.FromName(nameof(Data.Name)),
                    bindingMode = BindingMode.ToTarget
                });
                
                entityTextElement.SetBinding("text", new DataBinding()
                {
                    dataSourcePath = PropertyPath.FromName(nameof(Data.Entity)),
                    bindingMode = BindingMode.ToTarget
                });
            });
        }

        public void AddIncomingEdge(TweenEdgeView edgeView)
        {
            _incomingEdges.Add(edgeView);
        }
        
        public void AddOutgoingEdge(TweenEdgeView edgeView)
        {
            _outgoingEdges.Add(edgeView);
        }

        public Vector2 GetIncomingEdgePoint(TweenEdgeView edgeView)
        {
            return new Vector2(layout.xMin, GetEdgeYPoint(edgeView, _incomingEdges));
        }
        
        public Vector2 GetOutgoingEdgePoint(TweenEdgeView edgeView)
        {
            return new Vector2(layout.xMax, GetEdgeYPoint(edgeView, _outgoingEdges));
        }

        public void SortEdges()
        {
            _incomingEdges.Sort((a, b) => a.FromNodeView?.layout.yMin.CompareTo(b.FromNodeView?.layout.yMin) ?? 0);
            _outgoingEdges.Sort((a, b) => a.ToNodeView?.layout.yMin.CompareTo(b.ToNodeView?.layout.yMin) ?? 0);
        }

        private float GetEdgeYPoint(TweenEdgeView edgeView, List<TweenEdgeView> edges)
        {
            var index = edges.FindIndex(view => ReferenceEquals(edgeView, view));
            if (index < 0)
            {
                Debug.LogError($"Unknown edge: {edgeView.FromNodeView} -> {edgeView.ToNodeView}");
            }
            var param = index < 0 || edges.Count == 1 ? 0.5f : (float)index / (edges.Count - 1);
            var squashedParam = Mathf.Lerp(0.2f, 0.8f, param);
            var top = Mathf.Lerp(layout.yMin, layout.yMax, squashedParam);
            return top;
        }
    }
}
#endif