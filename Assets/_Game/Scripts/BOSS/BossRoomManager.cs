using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using PrismPanic.Core;
using PrismPanic.Utilities;

namespace PrismPanic.BOSS
{
    /// <summary>
    /// Manages the boss room's dynamic environment. Periodically shuffles
    /// pillars and mirrors to random positions, respecting placement rules:
    /// - Objects cannot be closer than BOSS_MIN_DISTANCE_FROM_BOSS to the boss.
    /// - Objects cannot be closer than BOSS_MIN_DISTANCE_BETWEEN_OBJECTS to each other.
    /// </summary>
    public class BossRoomManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private NavMeshSurface _navMeshSurface;

        [Header("Settings")]
        [SerializeField] private float _shuffleInterval = Constants.BOSS_SHUFFLE_INTERVAL;
        [SerializeField] private float _moveSpeed = Constants.BOSS_SHUFFLE_MOVE_SPEED;

        // Room bounds (computed from layout)
        private float _roomMinX, _roomMaxX, _roomMinZ, _roomMaxZ;
        private Vector3 _bossPosition;

        // Active objects being managed
        private readonly List<Transform> _pillars = new List<Transform>();
        private readonly List<Transform> _mirrors = new List<Transform>();

        // Movement targets
        private Vector3[] _pillarTargets;
        private Vector3[] _mirrorTargets;
        private float[] _mirrorTargetRotations;

        private float _shuffleTimer;
        private bool _isActive;

        private const int MAX_PLACEMENT_ATTEMPTS = 100; // Safety cap to avoid infinite loops

        /// <summary>
        /// Called by RoomConfigurator to initialize the boss room shuffler.
        /// </summary>
        public void Initialize(
            List<Transform> pillars,
            List<Transform> mirrors,
            Vector3 bossPosition,
            float roomMinX, float roomMaxX, float roomMinZ, float roomMaxZ,
            NavMeshSurface navMeshSurface)
        {
            _pillars.Clear();
            _pillars.AddRange(pillars);

            _mirrors.Clear();
            _mirrors.AddRange(mirrors);

            _bossPosition = bossPosition;
            _roomMinX = roomMinX;
            _roomMaxX = roomMaxX;
            _roomMinZ = roomMinZ;
            _roomMaxZ = roomMaxZ;
            _navMeshSurface = navMeshSurface;

            _pillarTargets = new Vector3[_pillars.Count];
            _mirrorTargets = new Vector3[_mirrors.Count];
            _mirrorTargetRotations = new float[_mirrors.Count];

            // Set initial targets to current positions
            for (int i = 0; i < _pillars.Count; i++)
                _pillarTargets[i] = _pillars[i].position;
            for (int i = 0; i < _mirrors.Count; i++)
            {
                _mirrorTargets[i] = _mirrors[i].position;
                _mirrorTargetRotations[i] = _mirrors[i].eulerAngles.y;
            }

            _shuffleTimer = _shuffleInterval;
            _isActive = true;
        }

        private void Update()
        {
            if (!_isActive) return;

            if (GameManager.Instance != null && GameManager.Instance.CurrentPhase != GamePhase.Combat)
                return;

            _shuffleTimer -= Time.deltaTime;
            if (_shuffleTimer <= 0f)
            {
                GenerateRandomPositions();
                _shuffleTimer = _shuffleInterval;
            }

            MoveTowardsTargets();
        }

        /// <summary>
        /// Generates new random positions for all pillars and mirrors
        /// respecting the minimum distance rules.
        /// </summary>
        private void GenerateRandomPositions()
        {
            List<Vector3> placedPositions = new List<Vector3>();

            float minFromBoss = Constants.BOSS_MIN_DISTANCE_FROM_BOSS;
            float minBetween = Constants.BOSS_MIN_DISTANCE_BETWEEN_OBJECTS;

            // --- Place pillars ---
            for (int i = 0; i < _pillars.Count; i++)
            {
                Vector3 pos = FindValidPosition(placedPositions, minFromBoss, minBetween);
                _pillarTargets[i] = pos;
                placedPositions.Add(pos);
            }

            // --- Place mirrors ---
            for (int i = 0; i < _mirrors.Count; i++)
            {
                Vector3 pos = FindValidPosition(placedPositions, minFromBoss, minBetween);
                _mirrorTargets[i] = pos;
                _mirrorTargetRotations[i] = Random.Range(0f, 360f); // Random rotation
                placedPositions.Add(pos);
            }

            Debug.Log("[BossRoom] Shuffled pillars and mirrors to random positions!");
        }

        /// <summary>
        /// Finds a random position inside the room bounds that satisfies:
        /// 1. At least minFromBoss distance from the boss center.
        /// 2. At least minBetween distance from all already-placed positions.
        /// </summary>
        private Vector3 FindValidPosition(List<Vector3> existingPositions, float minFromBoss, float minBetween)
        {
            // Inset by 1 unit to keep objects away from walls
            float inset = 1f;
            float xMin = _roomMinX + inset;
            float xMax = _roomMaxX - inset;
            float zMin = _roomMinZ + inset;
            float zMax = _roomMaxZ - inset;

            for (int attempt = 0; attempt < MAX_PLACEMENT_ATTEMPTS; attempt++)
            {
                float x = Random.Range(xMin, xMax);
                float z = Random.Range(zMin, zMax);
                Vector3 candidate = new Vector3(x, 1f, z); // y = 1 for object height

                // Rule 1: Must be far enough from the boss
                float distToBoss = Vector3.Distance(
                    new Vector3(candidate.x, 0, candidate.z),
                    new Vector3(_bossPosition.x, 0, _bossPosition.z)
                );
                if (distToBoss < minFromBoss) continue;

                // Rule 2: Must be far enough from every already-placed object
                bool tooClose = false;
                foreach (Vector3 placed in existingPositions)
                {
                    float dist = Vector3.Distance(
                        new Vector3(candidate.x, 0, candidate.z),
                        new Vector3(placed.x, 0, placed.z)
                    );
                    if (dist < minBetween)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                return candidate;
            }

            // Fallback: if no valid position found, pick a random one anyway
            Debug.LogWarning("[BossRoom] Could not find valid position after max attempts. Using fallback.");
            return new Vector3(Random.Range(xMin, xMax), 1f, Random.Range(zMin, zMax));
        }

        private void MoveTowardsTargets()
        {
            for (int i = 0; i < _pillars.Count; i++)
            {
                if (_pillars[i] == null) continue;
                if (Vector3.Distance(_pillars[i].position, _pillarTargets[i]) > 0.05f)
                {
                    _pillars[i].position = Vector3.MoveTowards(
                        _pillars[i].position, _pillarTargets[i], _moveSpeed * Time.deltaTime);
                }
            }

            for (int i = 0; i < _mirrors.Count; i++)
            {
                if (_mirrors[i] == null) continue;
                if (Vector3.Distance(_mirrors[i].position, _mirrorTargets[i]) > 0.05f)
                {
                    _mirrors[i].position = Vector3.MoveTowards(
                        _mirrors[i].position, _mirrorTargets[i], _moveSpeed * Time.deltaTime);
                }

                // Smoothly rotate mirrors
                float currentY = _mirrors[i].eulerAngles.y;
                float targetY = _mirrorTargetRotations[i];
                float newY = Mathf.MoveTowardsAngle(currentY, targetY, _moveSpeed * 30f * Time.deltaTime);
                _mirrors[i].rotation = Quaternion.Euler(0f, newY, 0f);
            }
        }

        public void StopShuffling()
        {
            _isActive = false;
        }
    }
}
