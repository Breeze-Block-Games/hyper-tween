#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;

namespace BreezeBlockGames.HyperTween.Editor.VisualElements
{
    [UxmlElement]
    public partial class ZoomView : VisualElement
    {
        private const float MinScale = 0.01f;
        private const float MaxScale = 1.00f;
        private const float ScaleStep = 4f;

        private float _scale = 1f;
        private Vector2 _translate = Vector2.zero;

        private Vector2 _lastMouse;
        private bool _isPanning;

        private readonly VisualElement _translateLayer;
        private readonly VisualElement _scaleLayer;

        public override VisualElement contentContainer => _scaleLayer;

        public ZoomView()
        {
            style.overflow = Overflow.Hidden;

            _translateLayer = new VisualElement { name = "translate-layer" };
            _translateLayer.StretchToParentSize();
            _translateLayer.style.overflow = new StyleEnum<Overflow>(Overflow.Visible);
            _translateLayer.style.position = Position.Absolute;

            hierarchy.Add(_translateLayer);

            _scaleLayer = new VisualElement { name = "scale-layer" };
            _scaleLayer.StretchToParentSize();
            _scaleLayer.style.overflow = new StyleEnum<Overflow>(Overflow.Visible);
            _scaleLayer.style.position = Position.Absolute;
            _scaleLayer.style.transformOrigin = new TransformOrigin(0, 0, 0);

            _translateLayer.Add(_scaleLayer);

            pickingMode = PickingMode.Position;
            RegisterCallback<WheelEvent>(OnWheel);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnWheel(WheelEvent e)
        {
            var ticks = e.delta.y / 120f;
            var newScale = Mathf.Clamp(_scale * Mathf.Pow(ScaleStep, -ticks), MinScale, MaxScale);

            if (Mathf.Approximately(newScale, _scale))
            {
                return;
            }

            var ratio = newScale / _scale;
            var cursor = this.WorldToLocal(e.mousePosition);

            // keep cursor-pivot fixed
            _translate = cursor - (cursor - _translate) * ratio;
            _scale = newScale;

            ApplyTransform();
            e.StopPropagation();
        }

        private void OnMouseDown(MouseDownEvent e)
        {
            _isPanning = true;
            _lastMouse = e.mousePosition;
            this.CaptureMouse();
            e.StopImmediatePropagation();
        }

        private void OnMouseMove(MouseMoveEvent e)
        {
            if (!_isPanning)
            {
                return;
            }

            _translate += e.mousePosition - _lastMouse;
            _lastMouse = e.mousePosition;
            ApplyTransform();
            e.StopPropagation();
        }

        private void OnMouseUp(MouseUpEvent e)
        {
            if (!_isPanning)
            {
                return;
            }

            _isPanning = false;
            this.ReleaseMouse();
        }

        private void ApplyTransform()
        {
            _translateLayer.style.translate = new Translate(_translate.x, _translate.y, 0);
            _scaleLayer.style.scale = new Vector3(_scale, _scale, 1f);
        }
    }
}
#endif