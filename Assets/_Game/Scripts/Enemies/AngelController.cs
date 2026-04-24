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

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        /// <summary>
        /// Called by RoomConfigurator when spawning from pool.
        /// </summary>
        public void Initialize(EnemyDataSO data)
        {
            _data = data;
            _currentHP = data != null ? data.maxHP : Constants.ANGEL_BASE_HP;
            _stunTimer = 0f;
            CurrentState = AngelState.Pursuing;

            if (_agent != null)
            {
                _agent.speed = data != null ? data.moveSpeed : Constants.ANGEL_BASE_SPEED;
                _agent.enabled = true;
                _agent.isStopped = false;
            }

            // Cache player reference
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                _playerTransform = player.transform;
        }

        private void Update()
        {
            if (CurrentState == AngelState.Dead) return;

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
            if (_playerTransform == null || _agent == null || !_agent.enabled) return;

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
            if (_agent != null && _agent.enabled)
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
            if (CurrentState == AngelState.Dead) return;

            if (other.gameObject.layer == Constants.LayerPlayer)
            {
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
