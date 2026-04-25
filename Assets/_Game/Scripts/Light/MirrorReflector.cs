using UnityEngine;
using UnityEngine.AI;

namespace PrismPanic.Light
{
    /// <summary>
    /// Component on Mirror prefab. Provides reflection normal for beam bouncing.
    /// The actual reflection math is done by BeamCaster — this is a data/tag component.
    /// Requires NavMeshObstacle so angels path around mirrors at runtime.
    /// </summary>
    [RequireComponent(typeof(NavMeshObstacle))]
    public class MirrorReflector : MonoBehaviour
    {
        /// <summary>
        /// Returns the mirror surface normal (forward direction of the transform).
        /// </summary>
        public Vector3 GetReflectionNormal()
        {
            return transform.forward;
        }

        private void Awake()
        {
            // Ensure the NavMeshObstacle is configured for dynamic carving
            var obstacle = GetComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            // Mirror visual is thin (0.1) but obstacle must be thick enough for NavMesh to carve
            obstacle.size = new Vector3(1.5f, 2f, 0.5f);
            obstacle.center = Vector3.zero;
            obstacle.carving = true;
            obstacle.carveOnlyStationary = false;
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
