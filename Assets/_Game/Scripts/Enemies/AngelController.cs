using UnityEngine;
using UnityEngine.AI;
using PrismPanic.Core;
using PrismPanic.Light;
using PrismPanic.ScriptableObjects;

namespace PrismPanic.Enemies
{
    /// <summary>
    /// Angel AI state machine. Freezes when illuminated, pursues player otherwise.
    /// States: Idle → Pursuing → Stunned → Dead.
    /// Uses NavMeshAgent for movement. Runs at Script Execution Order +50.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class AngelController : MonoBehaviour
    {
        public AngelState CurrentState { get; private set; } = AngelState.Idle;

        private NavMeshAgent _agent;
        private Transform _playerTransform;
        private EnemyDataSO _data;
        private int _currentHP;
        private float _stunTimer;
        private float _graceTimer; // brief invulnerability/idle after spawn
        
        [Header("VFX Settings")]
        [SerializeField] private GameObject deathEffectPrefab;
        [SerializeField] private float _vibrateAmount = 0.15f;

        [Header("Angel Type")]
        [Tooltip("If true, angel is invisible in darkness and only revealed by the flashlight.")]
        [SerializeField] private bool _isInvisibleType = false;

        private const float SPAWN_GRACE_DURATION = 1f; // seconds before angel starts moving
        private float _damageCooldownTimer;
        private float _vibrateTimer;
        private float _flashTimer; // Momentary flash when hitting player
        private Vector3 _basePosition;
        private Utilities.DirectionalSprite _directionalSprite;
        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer[] _allRenderers;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _directionalSprite = GetComponentInChildren<Utilities.DirectionalSprite>();
            if (_directionalSprite != null)
                _spriteRenderer = _directionalSprite.GetComponent<SpriteRenderer>();

            // Fallback: grab any SpriteRenderer in children if the above didn't find one
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            // Cache ALL renderers so we catch every sprite the DirectionalSprite might swap to
            _allRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);

            // Shadow type starts invisible; normal type is always visible
            SetRenderersEnabled(!_isInvisibleType);

            // Disable immediately — agent can't exist without NavMesh.
            if (_agent != null)
                _agent.enabled = false;
        }

        /// <summary>
        /// Called by RoomConfigurator AFTER NavMesh is baked.
        /// </summary>
        public void Initialize(EnemyDataSO data)
        {
            _data = data;
            _currentHP = data != null ? data.maxHP : Constants.ANGEL_BASE_HP;
            _stunTimer = 0f;
            _damageCooldownTimer = 0f;
            _vibrateTimer = 0f;
            _flashTimer = 0f;
            _graceTimer = SPAWN_GRACE_DURATION;
            CurrentState = AngelState.Idle;
            // Shadow angels start hidden; normal angels start visible
            SetRenderersEnabled(!_isInvisibleType);
            UpdateColorByHP();

            if (_agent != null)
            {
                _agent.speed = data != null ? data.moveSpeed : Constants.ANGEL_BASE_SPEED;

                // Find nearest valid NavMesh point — spawn positions may not be exactly on mesh
                NavMeshHit navHit;
                Vector3 targetPos = transform.position;
                if (NavMesh.SamplePosition(targetPos, out navHit, 5f, NavMesh.AllAreas))
                {
                    transform.position = navHit.position;
                    _agent.enabled = true;
                    _agent.Warp(navHit.position);
                    _agent.isStopped = true;
                }
                else
                {
                    // No NavMesh nearby — keep agent disabled, angel won't move but won't crash
                    Debug.LogWarning($"[Angel] No NavMesh within 5m of {targetPos}. Agent disabled.");
                    _agent.enabled = false;
                }
            }

            // Cache player reference
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                _playerTransform = player.transform;
        }

        private void Update()
        {
            if (CurrentState == AngelState.Dead) return;

            // Handle Timers and Visuals
            if (_damageCooldownTimer > 0f) _damageCooldownTimer -= Time.deltaTime;
            
            HandleVisualEffects();
            UpdateVisibility(); // MUST run last — always applies correct alpha on top of HP tint
            
            if (_directionalSprite != null)
            {
                _directionalSprite.SetEvilState(CurrentState == AngelState.Pursuing);
            }

            // Grace period — angel waits before pursuing
            if (_graceTimer > 0f)
            {
                _graceTimer -= Time.deltaTime;
                if (_graceTimer <= 0f)
                {
                    CurrentState = AngelState.Pursuing;
                    if (_agent != null && _agent.enabled)
                        _agent.isStopped = false;
                }
                return; // don't move or kill during grace
            }

            if (CurrentState == AngelState.Stunned)
            {
                HandleStunned();
                return;
            }

            // Check illumination — freeze if lit by beam
            if (AngelIlluminationRegistry.IsIlluminated(this))
            {
                StopMovement();
                return;
            }

            // Pursue player
            HandlePursuing();
        }

        private void HandlePursuing()
        {
            if (_playerTransform == null || _agent == null || !_agent.enabled || !_agent.isOnNavMesh) return;

            _agent.isStopped = false;
            _agent.SetDestination(_playerTransform.position);
        }

        private void HandleStunned()
        {
            StopMovement();

            _stunTimer -= Time.deltaTime;
            if (_stunTimer <= 0f)
            {
                CurrentState = AngelState.Pursuing;
            }
        }

        /// <summary>
        /// Enter stunned state for given duration. Called by BeamHitHandler.
        /// </summary>
        public void Stun(float duration)
        {
            if (CurrentState == AngelState.Dead) return;

            float modifier = _data != null ? _data.stunDurationModifier : 1f;
            _stunTimer = duration * modifier;
            CurrentState = AngelState.Stunned;
            StopMovement();

            EventBus.FireAngelStunned(gameObject);
        }

        /// <summary>
        /// Apply damage. Called by BeamHitHandler.
        /// </summary>
        public void TakeDamage(int amount)
        {
            if (CurrentState == AngelState.Dead || _damageCooldownTimer > 0f) return;

            _currentHP -= amount;
            _damageCooldownTimer = 0.3f; // 0.3s cooldown
            _vibrateTimer = 0.2f;

            if (_currentHP <= 0)
            {
                Die();
            }
            else
            {
                UpdateColorByHP();
            }
        }

        private void UpdateVisibility()
        {
            // Normal angels are always visible — skip
            if (!_isInvisibleType) return;

            bool illuminated = AngelIlluminationRegistry.IsIlluminated(this);
            SetRenderersEnabled(illuminated);
        }

        private void SetRenderersEnabled(bool visible)
        {
            if (_allRenderers == null) return;
            foreach (var sr in _allRenderers)
                if (sr != null) sr.enabled = visible;
        }

        private void UpdateColorByHP()
        {
            if (_spriteRenderer == null) return;
            float max = _data != null ? _data.maxHP : Constants.ANGEL_BASE_HP;
            float ratio = Mathf.Clamp01((float)_currentHP / max);
            _spriteRenderer.color = Color.Lerp(Color.red, Color.white, ratio);
        }

        private void HandleVisualEffects()
        {
            // Vibrate
            if (_vibrateTimer > 0f)
            {
                _vibrateTimer -= Time.deltaTime;
                Vector3 randomOffset = new Vector3(Random.Range(-_vibrateAmount, _vibrateAmount), 0, Random.Range(-_vibrateAmount, _vibrateAmount));
                
                // We vibrate the sprite child so we don't mess with the NavMeshAgent's transform
                if (_spriteRenderer != null)
                    _spriteRenderer.transform.localPosition = randomOffset;
            }
            else if (_spriteRenderer != null && _spriteRenderer.transform.localPosition != Vector3.zero)
            {
                _spriteRenderer.transform.localPosition = Vector3.zero;
            }

            // Momentary Flash when hitting player
            if (_flashTimer > 0f)
            {
                _flashTimer -= Time.deltaTime;
                if (_spriteRenderer != null)
                {
                    _spriteRenderer.color = Color.Lerp(Color.red, Color.white, Random.value);
                }
            }
            else if (_spriteRenderer != null && _flashTimer <= 0f && _flashTimer > -1f)
            {
                _flashTimer = -2f; // mark as done
                UpdateColorByHP(); // restore normal HP tint
            }
        }

        private void Die()
        {
            CurrentState = AngelState.Dead;
            StopMovement();

            // TODO: Play death VFX (particle burst) — Phase 8
            if (deathEffectPrefab != null)
            {
                GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }

            // Fire event before returning to pool
            EventBus.FireAngelKilled(gameObject);

            // Return to pool
            if (Utilities.PoolManager.Instance?.Angels != null)
            {
                _agent.enabled = false;
                Utilities.PoolManager.Instance.Angels.Return(transform);
            }
        }

        private void StopMovement()
        {
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Player contact deals damage. Using trigger collider.
        /// </summary>
        private void OnTriggerStay(Collider other)
        {
            if (CurrentState == AngelState.Dead || _graceTimer > 0f) return;

            if (other.CompareTag("Player") || other.gameObject.layer == Constants.LayerPlayer)
            {
                var player = other.GetComponent<PrismPanic.Player.PlayerController>();
                if (player != null && player.TakeDamage(1))
                {
                    // Player took damage (was not invincible) -> Angel visual feedback
                    _vibrateTimer = 0.2f;
                    _flashTimer = 0.2f;
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (CurrentState == AngelState.Dead || _graceTimer > 0f) return;

            if (collision.gameObject.CompareTag("Player") || collision.gameObject.layer == Constants.LayerPlayer)
            {
                var player = collision.gameObject.GetComponent<PrismPanic.Player.PlayerController>();
                if (player != null && player.TakeDamage(1))
                {
                    _vibrateTimer = 0.2f;
                    _flashTimer = 0.2f;
                }
            }
        }

        public int CurrentHP => _currentHP;
    }

    public enum AngelState
    {
        Idle,
        Pursuing,
        Stunned,
        Dead
    }
}
