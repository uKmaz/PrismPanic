using UnityEngine;

namespace PrismPanic.Light
{
    /// <summary>
    /// Component on Mirror prefab. Provides reflection normal for beam bouncing.
    /// The actual reflection math is done by BeamCaster — this is a data/tag component.
    /// </summary>
    public class MirrorReflector : MonoBehaviour
    {
        /// <summary>
        /// Returns the mirror surface normal (forward direction of the transform).
        /// </summary>
        public Vector3 GetReflectionNormal()
        {
            return transform.forward;
        }

        private void OnDrawGizmos()
        {
            // Draw normal direction in editor for easy mirror orientation
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, -transform.forward * 1.5f);
        }
    }
}
