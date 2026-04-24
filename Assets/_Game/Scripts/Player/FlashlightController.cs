using UnityEngine;
using UnityEngine.InputSystem;
using PrismPanic.Core;
using PrismPanic.ScriptableObjects;
using PrismPanic.Room;

namespace PrismPanic.Player
{
    /// <summary>
    /// Controls the flashlight beam activation and mirror placement mode.
    /// Child of the Player GameObject. Manages a URP Spot Light for visual cone.
    /// </summary>
    public class FlashlightController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerStatsSO _playerStats;
        [SerializeField] private UnityEngine.Light _spotLight;
        [SerializeField] private RoomConfigurator _roomConfigurator;
        [SerializeField] private GameObject _ghostMirrorPrefab;

        [Header("Settings")]
        [SerializeField] private float _placementDistance = 3f;

        private bool _isFlashlightActive = true; // ON by default
        private bool _isPlacementMode;
        private GameObject _ghostMirror;
        private float _placementRotation;

        // Cached
        private PlayerController _playerController;
        private Camera _mainCamera;
        private Plane _groundPlane = new Plane(Vector3.up, Vector3.zero);

        private void Awake()
        {
            _playerController = GetComponentInParent<PlayerController>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentPhase != GamePhase.Combat)
            {
                DeactivateFlashlight();
                return;
            }

            // Update spotlight cone angle from stats
            if (_spotLight != null)
            {
                _spotLight.spotAngle = _playerStats.flashlightConeAngle * 2f;
                _spotLight.enabled = _isFlashlightActive;
            }

            if (_isPlacementMode)
            {
                UpdateGhostMirror();
            }
        }

        // --- Input Callbacks ---

        public void OnFlashlight(InputValue value)
        {
            _isFlashlightActive = value.isPressed;
            if (!_isFlashlightActive)
            {
                DeactivateFlashlight();
            }
        }

        public void OnPlaceMirror(InputValue value)
        {
            if (!value.isPressed) return;

            if (_playerStats.placeableMirrorCount <= 0) return;

            if (!_isPlacementMode)
            {
                EnterPlacementMode();
            }
            else
            {
                ConfirmPlacement();
            }
        }

        // --- Placement Mode ---

        private void EnterPlacementMode()
        {
            _isPlacementMode = true;
            _placementRotation = 0f;

            if (_ghostMirrorPrefab != null && _ghostMirror == null)
            {
                _ghostMirror = Instantiate(_ghostMirrorPrefab);
                // Make it semi-transparent (handled by ghost material)
            }
        }

        private void UpdateGhostMirror()
        {
            if (_ghostMirror == null || _mainCamera == null) return;

            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (_groundPlane.Raycast(ray, out float distance))
            {
                Vector3 pos = ray.GetPoint(distance);
                pos.y = 0.5f; // Mirror height
                _ghostMirror.transform.position = pos;
                _ghostMirror.transform.rotation = Quaternion.Euler(0f, _placementRotation, 0f);
            }

            // Scroll wheel to rotate ghost mirror
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _placementRotation += scroll > 0 ? 15f : -15f;
            }
        }

        private void ConfirmPlacement()
        {
            if (_ghostMirror == null || _roomConfigurator == null) return;

            _roomConfigurator.PlaceExtraMirror(
                _ghostMirror.transform.position,
                _placementRotation
            );

            _playerStats.placeableMirrorCount--;
            ExitPlacementMode();
        }

        private void ExitPlacementMode()
        {
            _isPlacementMode = false;
            if (_ghostMirror != null)
            {
                Destroy(_ghostMirror);
                _ghostMirror = null;
            }
        }

        private void DeactivateFlashlight()
        {
            _isFlashlightActive = false;
            if (_spotLight != null)
                _spotLight.enabled = false;
        }

        public bool IsFlashlightActive => _isFlashlightActive;
    }
}
