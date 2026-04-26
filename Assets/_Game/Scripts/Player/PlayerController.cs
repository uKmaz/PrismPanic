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

        // Damage feedback
        private float _invincibilityTimer;
        private float _damageFlashTimer;
        private float _damageShakeTimer;
        private SpriteRenderer[] _spriteRenderers;
        private Color[] _originalColors;

        private const float DAMAGE_FLASH_DURATION = 0.3f;
        private const float DAMAGE_SHAKE_DURATION = 0.2f;
        private const float DAMAGE_SHAKE_AMOUNT = 0.1f;
        private const float INVINCIBILITY_BLINK_RATE = 10f; // Blinks per second

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _mainCamera = Camera.main;

            // Cache all sprite renderers for damage flash
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            _originalColors = new Color[_spriteRenderers.Length];
            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                _originalColors[i] = _spriteRenderers[i].color;
            }
        }

        private void Update()
        {
            if (_invincibilityTimer > 0f)
            {
                _invincibilityTimer -= Time.deltaTime;
                HandleInvincibilityVisuals();

                if (_invincibilityTimer <= 0f)
                {
                    // Invincibility ended — restore full visibility
                    RestoreOriginalColors();
                    SetRenderersVisible(true);
                }
            }

            if (_damageFlashTimer > 0f)
            {
                _damageFlashTimer -= Time.deltaTime;
                // Tint via MaterialPropertyBlock for 3DLit/URP shader compatibility
                float flashIntensity = _damageFlashTimer / DAMAGE_FLASH_DURATION;
                Color flashColor = Color.Lerp(Color.white, Color.red, flashIntensity);
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                foreach (var sr in _spriteRenderers)
                {
                    if (sr != null)
                    {
                        sr.GetPropertyBlock(block);
                        block.SetColor("_BaseColor", flashColor);
                        block.SetColor("_Color", flashColor);
                        sr.SetPropertyBlock(block);
                    }
                }

                if (_damageFlashTimer <= 0f)
                {
                    RestoreOriginalColors();
                }
            }

            if (_damageShakeTimer > 0f)
            {
                _damageShakeTimer -= Time.deltaTime;
                // Shake the sprite children (not the root — don't mess with CharacterController)
                foreach (var sr in _spriteRenderers)
                {
                    if (sr != null)
                    {
                        sr.transform.localPosition = new Vector3(
                            Random.Range(-DAMAGE_SHAKE_AMOUNT, DAMAGE_SHAKE_AMOUNT),
                            0f,
                            Random.Range(-DAMAGE_SHAKE_AMOUNT, DAMAGE_SHAKE_AMOUNT)
                        );
                    }
                }

                if (_damageShakeTimer <= 0f)
                {
                    // Reset sprite positions
                    foreach (var sr in _spriteRenderers)
                    {
                        if (sr != null)
                            sr.transform.localPosition = Vector3.zero;
                    }
                }
            }

            if (GameManager.Instance != null)
            {
                var phase = GameManager.Instance.CurrentPhase;
                // Allow movement during combat AND door selection
                if (phase != GamePhase.Combat && phase != GamePhase.DoorsOpen)
                    return;
            }

            HandleMovement();
            HandleAim();

            // Notify audio system of walking state
            bool isMoving = _moveInput.sqrMagnitude > 0.01f;
            Audio.AudioEffectHandler.Instance?.SetWalking(isMoving);
        }

        public bool TakeDamage(int amount)
        {
            if (_invincibilityTimer > 0f) return false;

            _playerStats.currentHP -= amount;
            _invincibilityTimer = 1.5f; // 1.5 seconds of invincibility
            _damageFlashTimer = DAMAGE_FLASH_DURATION;
            _damageShakeTimer = DAMAGE_SHAKE_DURATION;

            if (_playerStats.currentHP <= 0)
            {
                EventBus.FirePlayerDeath();
            }
            
            return true;
        }

        private void HandleInvincibilityVisuals()
        {
            // Blink on/off during invincibility
            bool visible = Mathf.Sin(Time.time * INVINCIBILITY_BLINK_RATE * Mathf.PI * 2f) > 0f;
            SetRenderersVisible(visible);
        }

        private void SetRenderersVisible(bool visible)
        {
            foreach (var sr in _spriteRenderers)
            {
                if (sr != null)
                    sr.enabled = visible;
            }
        }

        private void RestoreOriginalColors()
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                if (_spriteRenderers[i] != null)
                {
                    _spriteRenderers[i].color = _originalColors[i];
                    // Clear material property block to restore shader defaults
                    _spriteRenderers[i].GetPropertyBlock(block);
                    block.SetColor("_BaseColor", Color.white);
                    block.SetColor("_Color", Color.white);
                    _spriteRenderers[i].SetPropertyBlock(block);
                }
            }
        }

        private void HandleMovement()
        {
            // Convert 2D input to XZ movement (top-down — no gravity needed)
            Vector3 move = new Vector3(_moveInput.x, 0f, _moveInput.y);
            move = move.normalized * _playerStats.moveSpeed * Time.deltaTime;

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

