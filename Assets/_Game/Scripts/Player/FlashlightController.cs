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

        public FlashlightMode CurrentMode { get; private set; } = FlashlightMode.Closed;

        // Shield state (replaces old placement mode)
        private bool _isShieldActive;
        private GameObject _shieldMirror;
        private float _shieldRotation;

        /// <summary>
        /// True when the shield mirror is currently deployed.
        /// Read by ShieldUI to show on/off icon.
        /// </summary>
        public bool IsShieldActive => _isShieldActive;

        // Cached
        private PlayerController _playerController;
        private Camera _mainCamera;
        private Plane _groundPlane = new Plane(Vector3.up, Vector3.zero);

        private void Awake()
        {
            _playerController = GetComponentInParent<PlayerController>();
            _mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            CurrentMode = FlashlightMode.Closed;
            if (_spotLight != null)
                _spotLight.enabled = false;
        }

        private void Update()
        {
            if (GameManager.Instance != null)
            {
                var phase = GameManager.Instance.CurrentPhase;
                if (phase != GamePhase.Combat && phase != GamePhase.DoorsOpen)
                {
                    DeactivateFlashlight();
                    DeactivateShield();
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

                // Notify audio system of mode change
                Audio.AudioEffectHandler.Instance?.SetFlashlightMode(CurrentMode);
            }

            // Handle Shield Toggle via T key (requires hasShield upgrade)
            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
            {
                if (_playerStats.hasShield)
                {
                    if (!_isShieldActive) ActivateShield();
                    else DeactivateShield();
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
                    Audio.AudioEffectHandler.Instance?.SetFlashlightMode(CurrentMode);
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

            // Update shield mirror position if active
            if (_isShieldActive)
            {
                UpdateShieldMirror();
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
                Audio.AudioEffectHandler.Instance?.SetFlashlightMode(CurrentMode);
            }
        }

        public void OnPlaceMirror(InputValue value)
        {
            if (!value.isPressed) return;
            if (!_playerStats.hasShield) return;

            if (!_isShieldActive)
                ActivateShield();
            else
                DeactivateShield();
        }

        // --- Shield System (Toggle Mirror) ---

        private void ActivateShield()
        {
            _isShieldActive = true;
            _shieldRotation = 0f;

            if (_ghostMirrorPrefab != null && _shieldMirror == null)
            {
                _shieldMirror = Instantiate(_ghostMirrorPrefab);
            }

            if (_shieldMirror != null)
                _shieldMirror.SetActive(true);
        }

        private void UpdateShieldMirror()
        {
            if (_shieldMirror == null || _mainCamera == null) return;

            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (_groundPlane.Raycast(ray, out float distance))
            {
                Vector3 pos = ray.GetPoint(distance);
                pos.y = 0.5f; // Mirror height
                _shieldMirror.transform.position = pos;
                _shieldMirror.transform.rotation = Quaternion.Euler(0f, _shieldRotation, 0f);
            }

            // Scroll wheel to rotate shield mirror
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _shieldRotation += scroll > 0 ? 15f : -15f;
            }
        }

        private void DeactivateShield()
        {
            _isShieldActive = false;
            if (_shieldMirror != null)
            {
                Destroy(_shieldMirror);
                _shieldMirror = null;
            }
        }

        private void DeactivateFlashlight()
        {
            CurrentMode = FlashlightMode.Closed;
            if (_spotLight != null)
                _spotLight.enabled = false;
            Audio.AudioEffectHandler.Instance?.SetFlashlightMode(FlashlightMode.Closed);
        }

        public bool IsFlashlightActive => CurrentMode != FlashlightMode.Closed;
    }
}
