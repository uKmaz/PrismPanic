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

        private void Update()
        {
            // Clear illumination registry every frame
            AngelIlluminationRegistry.Clear();

            // Return previous segments to pool
            ReturnSegments();

            if (_flashlightController == null || !_flashlightController.IsFlashlightActive)
                return;

            if (GameManager.Instance != null && GameManager.Instance.CurrentPhase != GamePhase.Combat)
                return;

            // Cast beam from origin in player's forward direction
            Vector3 origin = _beamOrigin != null ? _beamOrigin.position : transform.position;
            Vector3 direction = _playerController != null
                ? _playerController.AimDirection
                : transform.forward;

            CastBeam(origin, direction, 0);
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
                    // Offset origin slightly to avoid re-hitting the same mirror
                    CastBeam(hit.point + reflectDir * 0.01f, reflectDir, bounceCount + 1);
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

            Transform segTransform = pool.BeamSegments.Get();
            BeamSegment seg = segTransform.GetComponent<BeamSegment>();
            if (seg != null)
            {
                seg.SetPositions(start, end);
                seg.SetWidth(Constants.BEAM_WIDTH);

                // Color by bounce count for visual feedback
                switch (bounceCount)
                {
                    case 0: seg.SetColor(Color.white); break;
                    case 1: seg.SetColor(Color.yellow); break;
                    case 2: seg.SetColor(new Color(1f, 0.5f, 0f)); break; // orange
                }
            }

            _activeSegments.Add(segTransform);
        }

        private void ReturnSegments()
        {
            var pool = PoolManager.Instance;
            if (pool?.BeamSegments == null) return;

            pool.BeamSegments.ReturnAll(_activeSegments);
        }
    }
}
