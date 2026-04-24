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

        private void Update()
        {
            // Clear illumination registry every frame
            AngelIlluminationRegistry.Clear();
            
            _currentSegmentCount = 0;

            if (_flashlightController == null || !_flashlightController.IsFlashlightActive)
            {
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

            CastBeam(origin, direction, 0);

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

        private void CastBeam(Vector3 origin, Vector3 direction, int bounceCount)
        {
            if (bounceCount > Constants.MAX_BOUNCES) return;

            float range = _playerStats != null ? _playerStats.beamRange : Constants.DEFAULT_BEAM_RANGE;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, range, Constants.BeamRaycastMask))
            {
                // Draw beam segment to hit point
                DrawSegment(origin, hit.point, bounceCount);

                int hitLayer = hit.collider.gameObject.layer;

                // Mirror — reflect and recurse
                if (hitLayer == Constants.LayerMirror)
                {
                    Vector3 reflectDir = Vector3.Reflect(direction, hit.normal);
                    // Keep reflection on XZ plane
                    reflectDir.y = 0f;
                    reflectDir.Normalize();
                    // Offset origin slightly to avoid re-hitting the same mirror
                    CastBeam(hit.point + reflectDir * 0.05f, reflectDir, bounceCount + 1);
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
                // No hit — draw to max range
                DrawSegment(origin, origin + direction * range, bounceCount);
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
                seg.ApplyStyle(bounceCount, Constants.BEAM_WIDTH);
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
