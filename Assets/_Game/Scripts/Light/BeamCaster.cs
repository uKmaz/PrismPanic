using System.Collections.Generic;
using UnityEngine;
using PrismPanic.Core;
using PrismPanic.Player;
using PrismPanic.Enemies;
using PrismPanic.ScriptableObjects;
using PrismPanic.Utilities;

namespace PrismPanic.Light
{
    /// <summary>
    /// Casts the flashlight beam, handles reflections off mirrors, and fires hit events.
    /// Runs at Script Execution Order -100 (before AngelController).
    /// </summary>
    public class BeamCaster : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerStatsSO _playerStats;
        [SerializeField] private FlashlightController _flashlightController;
        [SerializeField] private PlayerController _playerController;

        [Header("Beam Origin")]
        [SerializeField] private Transform _beamOrigin;

        // Active beam segments this frame
        private readonly List<Transform> _activeSegments = new List<Transform>();
        private int _currentSegmentCount = 0;
        private float _currentVisualRange = Constants.BEAM_START_LENGTH;

        private void Update()
        {
            // Clear illumination registry every frame
            AngelIlluminationRegistry.Clear();
            
            _currentSegmentCount = 0;

            if (_flashlightController == null || _flashlightController.CurrentMode == FlashlightMode.Closed)
            {
                _currentVisualRange = Constants.BEAM_START_LENGTH; // Fix: Reset beam growth when flashlight is turned off!
                ReturnSegments();
                return;
            }

            if (GameManager.Instance != null && GameManager.Instance.CurrentPhase != GamePhase.Combat)
            {
                ReturnSegments();
                return;
            }

            // Cast beam from origin in player's forward direction
            Vector3 origin = _beamOrigin != null 
                ? _beamOrigin.position 
                : (_playerController != null ? _playerController.transform.position : transform.position);
            
            // Force beam to a consistent height (mid-wall) for reliable hits
            origin.y = 1f;

            Vector3 direction = _playerController != null
                ? _playerController.AimDirection
                : transform.forward;
            // Flatten direction to XZ plane — beam travels horizontally
            direction.y = 0f;
            direction.Normalize();

            if (direction.sqrMagnitude < 0.001f) 
            {
                ReturnSegments();
                return; // no valid aim
            }

            // --- WIDE MODE ---
            if (_flashlightController.CurrentMode == FlashlightMode.Wide)
            {
                // Clean up any lingering laser segments
                ReturnSegments();
                
                float range = _playerStats != null ? _playerStats.beamRange : Constants.DEFAULT_BEAM_RANGE;
                float halfAngle = (_playerStats != null ? _playerStats.wideAngle : Constants.BASE_WIDE_ANGLE) / 2f;
                
                // 1. Broad phase: Find all enemies in a sphere
                Collider[] hits = Physics.OverlapSphere(origin, range, 1 << Constants.LayerEnemy);
                foreach (Collider hit in hits)
                {
                    Vector3 toEnemy = hit.transform.position - origin;
                    toEnemy.y = 0; // Flatten to XZ plane
                    
                    // 2. Narrow phase: Check if they are inside the mathematical cone angle
                    if (Vector3.Angle(direction, toEnemy.normalized) <= halfAngle)
                    {
                        // 3. Line of Sight phase: Make sure they aren't hiding behind a wall, pillar, or mirror
                        int blockMask = (1 << Constants.LayerWall) | (1 << Constants.LayerPillar) | (1 << Constants.LayerMirror);
                        if (!Physics.Raycast(origin, toEnemy.normalized, toEnemy.magnitude, blockMask))
                        {
                            AngelController angel = hit.GetComponent<AngelController>();
                            if (angel != null)
                            {
                                AngelIlluminationRegistry.Register(angel);
                                // Wide mode is always stun only (bounce 0), even with blue beam upgrade
                                EventBus.FireBeamHit(new BeamHitData(hit.gameObject, 0, hit.transform.position));
                            }
                        }
                    }
                }
                
                return; // Wide mode does NOT draw physical laser segments or reflect!
            }

            // Grow visual range for Laser Mode
            if (_flashlightController.CurrentMode == FlashlightMode.Laser)
            {
                float maxRange = _playerStats != null ? _playerStats.beamRange : Constants.DEFAULT_BEAM_RANGE;
                float growthSpeed = _playerStats != null ? _playerStats.beamGrowthSpeed : Constants.BEAM_GROWTH_SPEED;
                _currentVisualRange += growthSpeed * Time.deltaTime;
                if (_currentVisualRange > maxRange) _currentVisualRange = maxRange;
            }
            else
            {
                _currentVisualRange = Constants.BEAM_START_LENGTH;
            }

            // --- LASER MODE ---
            // Blue beam upgrade: start at bounce 1 so the first segment acts as blue-tier
            int startBounce = (_playerStats != null && _playerStats.hasBlueBeam) ? 1 : 0;
            CastBeam(origin, direction, startBounce, _currentVisualRange);

            if (_playerStats != null && _playerStats.multishotCount > 1)
            {
                Vector3 leftDir = Quaternion.Euler(0, -15f, 0) * direction;
                Vector3 rightDir = Quaternion.Euler(0, 15f, 0) * direction;
                CastBeam(origin, leftDir, startBounce, _currentVisualRange);
                CastBeam(origin, rightDir, startBounce, _currentVisualRange);
            }

            // After casting, return any excess segments to the pool that were not used this frame
            while (_activeSegments.Count > _currentSegmentCount)
            {
                int lastIndex = _activeSegments.Count - 1;
                Transform excessSeg = _activeSegments[lastIndex];
                
                var pool = PoolManager.Instance;
                if (pool?.BeamSegments != null)
                {
                    pool.BeamSegments.Return(excessSeg);
                }
                
                _activeSegments.RemoveAt(lastIndex);
            }
        }

        private void CastBeam(Vector3 origin, Vector3 direction, int bounceCount, float remainingRange)
        {
            if (bounceCount > Constants.MAX_BOUNCES || remainingRange <= 0f) return;

            float radius = _playerStats != null ? _playerStats.beamRadius : Constants.BASE_BEAM_RADIUS;

            if (Physics.SphereCast(origin, radius, direction, out RaycastHit hit, remainingRange, Constants.BeamRaycastMask))
            {
                // Draw beam segment to hit point
                DrawSegment(origin, hit.point, bounceCount);

                int hitLayer = hit.collider.gameObject.layer;
                float distanceTraveled = hit.distance;

                // Mirror — reflect and recurse
                if (hitLayer == Constants.LayerMirror)
                {
                    Vector3 reflectDir = Vector3.Reflect(direction, hit.normal);
                    // Keep reflection on XZ plane
                    reflectDir.y = 0f;
                    reflectDir.Normalize();
                    // Offset origin by radius to avoid re-hitting the same mirror
                    CastBeam(hit.point + reflectDir * (radius + 0.05f), reflectDir, bounceCount + 1, remainingRange - distanceTraveled);
                }
                // Enemy — register illumination and fire hit event
                else if (hitLayer == Constants.LayerEnemy)
                {
                    AngelController angel = hit.collider.GetComponent<AngelController>();
                    if (angel != null)
                    {
                        AngelIlluminationRegistry.Register(angel);

                        EventBus.FireBeamHit(new BeamHitData(
                            hit.collider.gameObject,
                            bounceCount,
                            hit.point
                        ));
                    }
                }
                // Wall or Pillar — beam terminates (segment already drawn)
            }
            else
            {
                // No hit — draw to remaining range
                DrawSegment(origin, origin + direction * remainingRange, bounceCount);
            }
        }

        private void DrawSegment(Vector3 start, Vector3 end, int bounceCount)
        {
            var pool = PoolManager.Instance;
            if (pool?.BeamSegments == null) return;

            Transform segTransform;
            
            // Reuse an already active segment if possible to prevent ParticleSystem reset
            if (_currentSegmentCount < _activeSegments.Count)
            {
                segTransform = _activeSegments[_currentSegmentCount];
            }
            else
            {
                segTransform = pool.BeamSegments.Get();
                _activeSegments.Add(segTransform);
            }

            BeamSegment seg = segTransform.GetComponent<BeamSegment>();
            if (seg != null)
            {
                seg.SetPositions(start, end);

                // Calculate visual width from beam radius
                float radius = _playerStats != null ? _playerStats.beamRadius : Constants.BASE_BEAM_RADIUS;
                float visualWidth = Mathf.Max(Constants.BEAM_WIDTH, radius * 1.2f);

                // Check if the first segment should be blue
                bool forceBlue = bounceCount == 0 && (_playerStats != null && _playerStats.hasBlueBeam);

                seg.ApplyStyle(bounceCount, visualWidth, forceBlue);
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
    }
}
