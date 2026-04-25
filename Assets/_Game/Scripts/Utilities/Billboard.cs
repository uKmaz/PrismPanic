using UnityEngine;

namespace PrismPanic.Utilities
{
    /// <summary>
    /// Makes a 2D sprite always face the camera in a 3D world.
    /// Attach to the child GameObject that has the SpriteRenderer.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        private enum BillboardMode
        {
            /// <summary>Fully faces camera (good for top-down)</summary>
            FaceCamera,
            /// <summary>Only rotates on Y axis (good for side-scrolling look in 3D)</summary>
            YAxisOnly
        }

        [SerializeField] private BillboardMode _mode = BillboardMode.FaceCamera;
        [SerializeField] private bool _flipWithParent = true;

        private Camera _mainCamera;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null) return;
            }

            switch (_mode)
            {
                case BillboardMode.FaceCamera:
                    // Match camera rotation exactly — sprite always faces screen
                    transform.rotation = _mainCamera.transform.rotation;
                    break;

                case BillboardMode.YAxisOnly:
                    // Only rotate on Y axis — keeps sprite upright
                    Vector3 camForward = _mainCamera.transform.forward;
                    camForward.y = 0f;
                    if (camForward.sqrMagnitude > 0.001f)
                    {
                        transform.rotation = Quaternion.LookRotation(camForward, Vector3.up);
                    }
                    break;
            }

            // Flip sprite based on parent's movement direction (optional)
            if (_flipWithParent && _spriteRenderer != null)
            {
                Vector3 parentForward = transform.parent != null
                    ? transform.parent.forward
                    : Vector3.forward;

                // Convert parent forward to camera-relative left/right
                Vector3 camRight = _mainCamera.transform.right;
                float dot = Vector3.Dot(parentForward, camRight);
                _spriteRenderer.flipX = dot < 0f;
            }
        }
    }
}
