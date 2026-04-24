using UnityEngine;
using UnityEngine.InputSystem;
using PrismPanic.Core;
using PrismPanic.ScriptableObjects;

namespace PrismPanic.Player
{
    /// <summary>
    /// Handles player movement on XZ plane and aim direction toward mouse.
    /// Uses Unity Input System and CharacterController.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerStatsSO _playerStats;

        private CharacterController _characterController;
        private Camera _mainCamera;

        // Input
        private Vector2 _moveInput;
        private Vector3 _aimWorldPos;

        // Cached ground plane for mouse raycast
        private Plane _groundPlane = new Plane(Vector3.up, Vector3.zero);

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentPhase != GamePhase.Combat)
                return;

            HandleMovement();
            HandleAim();
        }

        private void HandleMovement()
        {
            // Convert 2D input to XZ movement
            Vector3 move = new Vector3(_moveInput.x, 0f, _moveInput.y);
            move = move.normalized * _playerStats.moveSpeed * Time.deltaTime;

            // Apply gravity
            if (!_characterController.isGrounded)
            {
                move.y -= 9.81f * Time.deltaTime;
            }

            _characterController.Move(move);
        }

        private void HandleAim()
        {
            if (_mainCamera == null) return;

            // Raycast from camera through mouse to ground plane
            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (_groundPlane.Raycast(ray, out float distance))
            {
                _aimWorldPos = ray.GetPoint(distance);
                _aimWorldPos.y = transform.position.y; // Keep same height

                Vector3 lookDir = _aimWorldPos - transform.position;
                if (lookDir.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
                }
            }
        }

        // --- Input System callbacks (assign in PlayerInput component or via code) ---

        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }

        public Vector3 AimWorldPosition => _aimWorldPos;
        public Vector3 AimDirection => (AimWorldPosition - transform.position).normalized;
    }
}
