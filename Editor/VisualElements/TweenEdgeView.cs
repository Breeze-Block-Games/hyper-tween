#if UNITY_EDITOR
using System;
using BreezeBlockGames.HyperTween.Editor.TweenVisualizer;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace BreezeBlockGames.HyperTween.Editor.VisualElements
{
    [UxmlElement]
    public partial class TweenEdgeView : VisualElement
    {
        public class Data
        {
            [CreateProperty]
            public State State { get; set; }
            [CreateProperty]
            public Type Type { get; set; }
            [CreateProperty]
            public float Progress { get; set; }
        }
        
        public enum State
        {
            None,
            Created,
            Completed,
            Conflicted
        }

        public enum Type
        {
            None,
            Duration,
            ForkOnPlay,
            SignalJoinOnStop,
            PlayOnPlay,
            PlayOnStop
        }
        
        private State _currentState;
        
        [CreateProperty]
        private State CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                _lineColor = value switch
                {
                    State.None => Color.black,
                    State.Created => Color.gray,
                    State.Conflicted => Color.red,
                    State.Completed => GetTypeColor(CurrentData.Type),
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
                };
                MarkDirtyRepaint();
            }
        }

        private float _currentProgress;
        
        [CreateProperty]
        public float CurrentProgress
        {
            get => _currentProgress;
            set
            {
                _currentProgress = value;
                MarkDirtyRepaint();
            }
        }

        private Type _currentType;
        
        [CreateProperty]
        private Type CurrentType
        {
            get => _currentType;
            set
            {
                _currentType = value;
                _lineColor = GetTypeColor(value);
                MarkDirtyRepaint();
            }
        }

        private Color GetTypeColor(Type type)
        {
            return type switch
            {
                Type.None => Color.clear,
                Type.Duration => Color.yellow,
                Type.ForkOnPlay => Color.magenta,
                Type.SignalJoinOnStop => Color.blue,
                Type.PlayOnPlay => Color.green,
                Type.PlayOnStop => Color.cyan,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        
        public Data CurrentData { get; } = new();
        public TweenNodeView? FromNodeView { get; set; }
        public TweenNodeView? ToNodeView { get; set; }
        public TweenJournalGraphVisualizer.EdgeKey Key { get; set; }

        private const float BezierStrength = 40f;
        private const float LineWidth = 3f;
        private readonly Color _backgroundColor = Color.grey;

        private Color _lineColor;
        
        public TweenEdgeView()
        {
            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            dataSource = CurrentData;
            
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
            
            SetBinding(nameof(CurrentProgress), new DataBinding()
            {
                dataSourcePath = new PropertyPath(nameof(Data.Progress)),
                dataSourceType = typeof(float)
            });
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            MarkDirtyRepaint();
        }

        private static void SplitCubicBezier(
            Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t,
            // 0 → t
            out Vector2 q0, out Vector2 q1, out Vector2 q2, out Vector2 q3,     
            // t → 1
            out Vector2 r0, out Vector2 r1, out Vector2 r2, out Vector2 r3)       
        {
            // de Casteljau
            var a = Vector2.Lerp(p0, p1, t);
            var b = Vector2.Lerp(p1, p2, t);
            var c = Vector2.Lerp(p2, p3, t);

            var d = Vector2.Lerp(a, b, t);
            var e = Vector2.Lerp(b, c, t);

            var f = Vector2.Lerp(d, e, t);   // point on original curve at t

            // first segment (0 → t)
            q0 = p0;   q1 = a;   q2 = d;   q3 = f;

            // second segment (t → 1)
            r0 = f;    r1 = e;   r2 = c;   r3 = p3;
        }

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            if (FromNodeView == null || ToNodeView == null)
            {
                return;
            }

            var painter = ctx.painter2D;

            var p0 = FromNodeView.GetOutgoingEdgePoint(this);
            var p3 = ToNodeView.GetIncomingEdgePoint(this);

            // Tangent control points (horizontal)
            var p1 = p0 + Vector2.right * BezierStrength;
            var p2 = p3 + Vector2.left  * BezierStrength;

            // Early-outs
            var t = Mathf.Clamp01(_currentProgress);
            if (Mathf.Approximately(t, 0f))
            {
                // whole curve is “background”
                DrawSegment(painter, _backgroundColor, p0, p1, p2, p3);
                DrawArrowHead(painter, p2, p3, _lineColor);
                return;
            }
            if (Mathf.Approximately(t, 1f))
            {
                // whole curve is “filled”
                DrawSegment(painter, _lineColor, p0, p1, p2, p3);
                DrawArrowHead(painter, p2, p3, _lineColor);
                return;
            }

            // Split at t
            SplitCubicBezier(p0, p1, p2, p3, t,
                             out var q0, out var q1, out var q2, out var q3,
                             out var r0, out var r1, out var r2, out var r3);

            // Draw 0 → t in fill colour
            DrawSegment(painter, _lineColor, q0, q1, q2, q3);

            // Draw t → 1 in background colour
            DrawSegment(painter, _backgroundColor, r0, r1, r2, r3);

            // Arrow takes the colour of its segment (background part)
            DrawArrowHead(painter, r2, r3, _lineColor);
        }

        private static void DrawSegment(Painter2D painter, Color col, Vector2 s0, Vector2 s1, Vector2 s2, Vector2 s3)
        {
            painter.strokeColor = col;
            painter.lineWidth   = LineWidth;
            painter.lineJoin    = LineJoin.Round;

            painter.BeginPath();
            painter.MoveTo(s0);
            painter.BezierCurveTo(s1, s2, s3);
            painter.Stroke();
        }

        private static void DrawArrowHead(Painter2D painter, Vector2 tangentCtrl, Vector2 tip, Color col)
        {
            const float size = 8f;

            var dir = (tip - tangentCtrl).normalized;

            var left  = tip - dir * size + Vector2.Perpendicular(dir) * 0.5f * size;
            var right = tip - dir * size - Vector2.Perpendicular(dir) * 0.5f * size;

            painter.strokeColor = col;
            painter.lineWidth   = LineWidth;
            painter.lineJoin    = LineJoin.Round;

            painter.BeginPath();
            painter.MoveTo(left);
            painter.LineTo(tip);
            painter.LineTo(right);
            painter.Stroke();
        }
    }
}
#endif