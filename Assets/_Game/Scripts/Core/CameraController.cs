using UnityEngine;

namespace PrismPanic.Core
{
    /// <summary>
    /// Isometric camera that smoothly follows the player.
    /// Attach to Main Camera. Assign player transform in Inspector.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;

        [Header("Isometric Settings")]
        [Tooltip("Offset from player position")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 14f, -10f);

        [Tooltip("How fast camera follows")]
        [SerializeField] private float _smoothSpeed = 8f;

        [Header("Camera Angle")]
        [Tooltip("X rotation for isometric look (55 = good isometric)")]
        [SerializeField] private float _pitch = 55f;

        private void Start()
        {
            // Set initial rotation
            transform.rotation = Quaternion.Euler(_pitch, 0f, 0f);

            // Snap to target immediately on start
            if (_target != null)
            {
                transform.position = _target.position + _offset;
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desiredPos = _target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, desiredPos, _smoothSpeed * Time.deltaTime);
        }
    }
}
