using System.Collections.Generic;
using UnityEngine;
using PrismPanic.Core;
using PrismPanic.Player;
using PrismPanic.Utilities;

namespace PrismPanic.Enemies
{
    /// <summary>
    /// Boss Controller for the Boss Fight map (Map 9).
    /// Stationary boss at room center. Continuously rotates and fires laser beams
    /// that reflect off mirrors, get stronger, and change color — just like the player's.
    /// Starts with 1 ray, gains +1 every BOSS_RAY_ESCALATION_INTERVAL up to BOSS_MAX_RAY_COUNT.
    /// </summary>
    public class BossController : MonoBehaviour
    {
        [Header("Boss Stats")]
        [SerializeField] private int _maxHealth = Constants.BOSS_MAX_HP;

        [Header("Laser Settings")]
        [SerializeField] private float _rotationSpeed = Constants.BOSS_ROTATION_SPEED;
        [SerializeField] private float _laserRange = Constants.BOSS_LASER_RANGE;
        [SerializeField] private int _laserDamage = Constants.BOSS_LASER_DAMAGE;
        [SerializeField] private float _laserWidth = Constants.BOSS_LASER_WIDTH;

        private int _currentHealth;
        private bool _isDead;
        private float _damageCooldownTimer;

        // Ray escalation
        private int _currentRayCount = 1;
        private float _escalationTimer;

        // Laser firing cycle
        private float _strikeTimer;
        private bool _isFiring;

        // Active beam segments used by the boss laser
        private readonly List<Transform> _activeSegments = new List<Transform>();
        private int _currentSegmentCount;

        // Total combat time for escalation
        private float _totalCombatTime;

        private void OnEnable()
        {
            // Always read from Constants to avoid stale serialized values in the prefab
            _maxHealth = Constants.BOSS_MAX_HP;
            _currentHealth = _maxHealth;
            _isDead = false;
            _damageCooldownTimer = 0f;
            _currentRayCount = 1;
            _totalCombatTime = 0f;
            _escalationTimer = Constants.BOSS_RAY_ESCALATION_INTERVAL;
            _strikeTimer = Constants.BOSS_LASER_STRIKE_DURATION;
            _isFiring = true;
        }

        private void Update()
        {
            if (_isDead) return;

            if (_damageCooldownTimer > 0f) _damageCooldownTimer -= Time.deltaTime;

            if (GameManager.Instance != null && GameManager.Instance.CurrentPhase != GamePhase.Combat)
            {
                ReturnSegments();
                return;
            }

            // Track total combat time for escalation
            _totalCombatTime += Time.deltaTime;

            // Ray escalation: every N seconds, add a ray
            _escalationTimer -= Time.deltaTime;
            if (_escalationTimer <= 0f && _currentRayCount < Constants.BOSS_MAX_RAY_COUNT)
            {
                _currentRayCount++;
                _escalationTimer = Constants.BOSS_RAY_ESCALATION_INTERVAL;
                Debug.Log($"[Boss] Ray count escalated to {_currentRayCount}!");
            }

            // Rotation — always rotating
            HandleRotation();

            // Laser firing cycle: fire for duration, then cooldown
            _strikeTimer -= Time.deltaTime;
            if (_isFiring)
            {
                if (_strikeTimer <= 0f)
                {
                    // Stop firing, start cooldown — reset ray count for next strike
                    _isFiring = false;
                    _strikeTimer = Constants.BOSS_LASER_COOLDOWN;
                    _currentRayCount = 1;
                    _escalationTimer = Constants.BOSS_RAY_ESCALATION_INTERVAL;
                    ReturnSegments();
                    return;
                }

                HandleLaser();
            }
            else
            {
                ReturnSegments();
                if (_strikeTimer <= 0f)
                {
                    // Start firing again
                    _isFiring = true;
                    _strikeTimer = Constants.BOSS_LASER_STRIKE_DURATION;
                }
            }
        }

        private void HandleRotation()
        {
            transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
        }

        private void HandleLaser()
        {
            _currentSegmentCount = 0;

            Vector3 origin = transform.position;
            origin.y = 1f; // Match beam height

            // Fixed ray offsets relative to boss forward:
            // Ray 1: 0° (forward)
            // Ray 2: 180° (opposite)
            // Ray 3: 90° (right)
            // Ray 4: 270° (left)
            float[] rayOffsets = { 0f, 180f, 90f, 270f };

            for (int i = 0; i < _currentRayCount; i++)
            {
                Vector3 direction = Quaternion.Euler(0, rayOffsets[i], 0) * transform.forward;
                direction.y = 0f;
                direction.Normalize();

                CastBossBeam(origin, direction, 0, _laserRange);
            }

            // Return excess segments
            while (_activeSegments.Count > _currentSegmentCount)
            {
                int lastIndex = _activeSegments.Count - 1;
                Transform seg = _activeSegments[lastIndex];
                var pool = PoolManager.Instance;
                if (pool?.BeamSegments != null)
                    pool.BeamSegments.Return(seg);
                _activeSegments.RemoveAt(lastIndex);
            }
        }

        /// <summary>
        /// Recursive beam cast — mirrors reflect, enemies/player get hit.
        /// Uses the same visual system as the player's BeamCaster.
        /// </summary>
        private void CastBossBeam(Vector3 origin, Vector3 direction, int bounceCount, float remainingRange)
        {
            if (bounceCount > Constants.MAX_BOUNCES || remainingRange <= 0f) return;

            // Boss laser hits walls, pillars, mirrors, enemies, AND the player
            int hitMask = Constants.BeamRaycastMask | (1 << Constants.LayerPlayer);

            if (Physics.Raycast(origin, direction, out RaycastHit hit, remainingRange, hitMask))
            {
                DrawSegment(origin, hit.point, bounceCount);

                int hitLayer = hit.collider.gameObject.layer;
                float distanceTraveled = hit.distance;

                // Mirror — reflect and recurse (beam gets stronger!)
                if (hitLayer == Constants.LayerMirror)
                {
                    Vector3 reflectDir = Vector3.Reflect(direction, hit.normal);
                    reflectDir.y = 0f;
                    reflectDir.Normalize();
                    CastBossBeam(hit.point + reflectDir * 0.05f, reflectDir, bounceCount + 1, remainingRange - distanceTraveled);
                }
                // Player — deal damage
                else if (hitLayer == Constants.LayerPlayer)
                {
                    PlayerController player = hit.collider.GetComponent<PlayerController>();
                    if (player != null)
                    {
                        player.TakeDamage(_laserDamage);
                    }
                }
                // Enemy (angel) — the boss laser doesn't hurt other angels, just passes through visually
                // Wall/Pillar — beam terminates (segment already drawn)
            }
            else
            {
                DrawSegment(origin, origin + direction * remainingRange, bounceCount);
            }
        }

        private void DrawSegment(Vector3 start, Vector3 end, int bounceCount)
        {
            var pool = PoolManager.Instance;
            if (pool?.BeamSegments == null) return;

            Transform segTransform;

            if (_currentSegmentCount < _activeSegments.Count)
            {
                segTransform = _activeSegments[_currentSegmentCount];
            }
            else
            {
                segTransform = pool.BeamSegments.Get();
                _activeSegments.Add(segTransform);
            }

            Light.BeamSegment seg = segTransform.GetComponent<Light.BeamSegment>();
            if (seg != null)
            {
                seg.SetPositions(start, end);
                // Boss beam starts orange (bounce 0), then follows normal color escalation
                seg.ApplyStyle(bounceCount, _laserWidth, false);
            }

            // Override bounce-0 color to boss orange-red
            if (bounceCount == 0)
            {
                var lr = segTransform.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    Color bossColor = new Color(1f, 0.3f, 0f); // Orange-red
                    float hdr = 6f;
                    Color hdrColor = new Color(bossColor.r * hdr, bossColor.g * hdr, bossColor.b * hdr, 1f);
                    lr.startColor = bossColor;
                    lr.endColor = bossColor;
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    lr.GetPropertyBlock(block);
                    block.SetColor("_BaseColor", hdrColor);
                    block.SetColor("_Color", hdrColor);
                    lr.SetPropertyBlock(block);
                }
            }

            _currentSegmentCount++;
        }

        private void ReturnSegments()
        {
            var pool = PoolManager.Instance;
            if (pool?.BeamSegments == null) return;

            pool.BeamSegments.ReturnAll(_activeSegments);
            _activeSegments.Clear();
            _currentSegmentCount = 0;
        }

        /// <summary>
        /// Called by BeamHitHandler when a player's reflected beam hits the boss.
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (_isDead || _damageCooldownTimer > 0f) return;

            _currentHealth -= damage;
            _damageCooldownTimer = 0.3f; // 0.3s cooldown — matches Angel
            Debug.Log($"[Boss] Took {damage} damage! HP left: {_currentHealth}");

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;
        public int CurrentRayCount => _currentRayCount;

        private void Die()
        {
            _isDead = true;
            ReturnSegments();
            Debug.Log("[Boss] Defeated!");
            EventBus.FireVictory();
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            ReturnSegments();
        }
    }
}
