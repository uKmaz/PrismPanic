using UnityEngine;

namespace PrismPanic.Light
{
    /// <summary>
    /// Pooled beam segment with LineRenderer. Renders one segment of the beam.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class BeamSegment : MonoBehaviour
    {
        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.positionCount = 2;
        }

        public void SetPositions(Vector3 start, Vector3 end)
        {
            _lineRenderer.SetPosition(0, start);
            _lineRenderer.SetPosition(1, end);
        }

        public void SetWidth(float width)
        {
            _lineRenderer.startWidth = width;
            _lineRenderer.endWidth = width;
        }

        public void SetColor(Color color)
        {
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
        }
    }
}
