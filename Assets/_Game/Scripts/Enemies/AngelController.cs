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

        private const float SPAWN_GRACE_DURATION = 1f; // seconds before angel starts moving

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            // Disable immediately — agent can't exist without NavMesh.
            // It will be enabled in Initialize() after NavMesh is baked.
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
            _graceTimer = SPAWN_GRACE_DURATION;
            CurrentState = AngelState.Idle;

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
            if (CurrentState == AngelState.Dead) return;

            _currentHP -= amount;
            if (_currentHP <= 0)
            {
                Die();
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
        /// Player contact = instant death. Using trigger collider.
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (CurrentState == AngelState.Dead || _graceTimer > 0f) return;

            bool isPlayer = other.gameObject.layer == Constants.LayerPlayer
                         || other.CompareTag("Player");

            if (isPlayer)
            {
                EventBus.FirePlayerDeath();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (CurrentState == AngelState.Dead || _graceTimer > 0f) return;

            bool isPlayer = collision.gameObject.layer == Constants.LayerPlayer
                         || collision.gameObject.CompareTag("Player");

            if (isPlayer)
            {
                Debug.Log("[Angel] Player collision! Firing PlayerDeath.");
                EventBus.FirePlayerDeath();
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
