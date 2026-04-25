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

        public FlashlightMode CurrentMode { get; private set; } = FlashlightMode.Wide;

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
            if (GameManager.Instance != null)
            {
                var phase = GameManager.Instance.CurrentPhase;
                if (phase != GamePhase.Combat && phase != GamePhase.DoorsOpen)
                {
                    DeactivateFlashlight();
                    return;
                }
            }

            // Handle Overheat Recovery
            if (_playerStats.isOverheated)
            {
                if (_playerStats.currentEnergy >= _playerStats.maxEnergy * Constants.ENERGY_OVERHEAT_THRESHOLD)
                {
                    _playerStats.isOverheated = false;
                }
                else
                {
                    CurrentMode = FlashlightMode.Closed; // Lock in closed state
                }
            }

            // Handle mode toggle via R key
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame && !_playerStats.isOverheated)
            {
                if (CurrentMode == FlashlightMode.Closed) CurrentMode = FlashlightMode.Wide;
                else if (CurrentMode == FlashlightMode.Wide) CurrentMode = FlashlightMode.Laser;
                else CurrentMode = FlashlightMode.Closed;
            }

            // Handle Mirror Placement via T key
            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
            {
                if (_playerStats.placeableMirrorCount > 0)
                {
                    if (!_isPlacementMode) EnterPlacementMode();
                    else ConfirmPlacement();
                }
            }

            // Energy Logic
            if (CurrentMode == FlashlightMode.Closed)
            {
                _playerStats.currentEnergy += Constants.ENERGY_REGEN_RATE * Time.deltaTime;
                if (_playerStats.currentEnergy > _playerStats.maxEnergy) 
                    _playerStats.currentEnergy = _playerStats.maxEnergy;
            }
            else
            {
                float drain = CurrentMode == FlashlightMode.Wide ? Constants.ENERGY_DRAIN_WIDE : Constants.ENERGY_DRAIN_LASER;
                drain *= _playerStats.energyDrainMultiplier;
                _playerStats.currentEnergy -= drain * Time.deltaTime;

                if (_playerStats.currentEnergy <= 0)
                {
                    _playerStats.currentEnergy = 0;
                    _playerStats.isOverheated = true;
                    CurrentMode = FlashlightMode.Closed; // Force close when out of energy
                }
            }

            // Update spotlight cone angle and intensity smoothly
            if (_spotLight != null)
            {
                bool isActive = CurrentMode != FlashlightMode.Closed;
                _spotLight.enabled = isActive;
                
                if (isActive)
                {
                    float targetAngle = CurrentMode == FlashlightMode.Wide ? _playerStats.wideAngle : _playerStats.laserAngle;
                    float targetIntensity = CurrentMode == FlashlightMode.Wide ? Constants.BASE_WIDE_INTENSITY : Constants.BASE_LASER_INTENSITY; 

                    _spotLight.spotAngle = Mathf.Lerp(_spotLight.spotAngle, targetAngle, Time.deltaTime * 15f);
                    _spotLight.intensity = Mathf.Lerp(_spotLight.intensity, targetIntensity, Time.deltaTime * 15f);
                }
            }

            if (_isPlacementMode)
            {
                UpdateGhostMirror();
            }
        }

        // --- Input Callbacks ---

        public void OnFlashlight(InputValue value)
        {
            if (value.isPressed)
            {
                // Optionally let left-click force it on if it's closed and NOT overheated
                if (CurrentMode == FlashlightMode.Closed && !_playerStats.isOverheated)
                {
                    CurrentMode = FlashlightMode.Wide;
                }
            }
            else
            {
                CurrentMode = FlashlightMode.Closed;
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
            CurrentMode = FlashlightMode.Closed;
            if (_spotLight != null)
                _spotLight.enabled = false;
        }

        public bool IsFlashlightActive => CurrentMode != FlashlightMode.Closed;
    }
}
