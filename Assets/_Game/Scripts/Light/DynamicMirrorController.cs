using UnityEngine;
using System.Collections.Generic;

namespace PrismPanic.Light
{
    /// <summary>
    /// Makes a mirror move between multiple predefined waypoints over time.
    /// Useful for Boss maps where mirrors change positions to create dynamic beam puzzles.
    /// </summary>
    public class DynamicMirrorController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("The positions this mirror will travel between.")]
        [SerializeField] private List<Vector3> _waypoints = new List<Vector3>();
        [SerializeField] private float _moveSpeed = 2f;
        
        [Tooltip("How long to wait at each waypoint before moving to the next.")]
        [SerializeField] private float _waitTimeAtWaypoint = 2f;

        private int _currentWaypointIndex = 0;
        private float _waitTimer = 0f;
        private bool _isMoving = false;

        private void Start()
        {
            if (_waypoints.Count > 0)
            {
                // Start at the first waypoint
                transform.position = _waypoints[0];
                _waitTimer = _waitTimeAtWaypoint;
            }
        }

        private void Update()
        {
            if (_waypoints.Count <= 1) return;

            if (_isMoving)
            {
                MoveTowardsTarget();
            }
            else
            {
                HandleWaitTimer();
            }
        }

        private void MoveTowardsTarget()
        {
            Vector3 target = _waypoints[_currentWaypointIndex];
            transform.position = Vector3.MoveTowards(transform.position, target, _moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.05f)
            {
                transform.position = target;
                _isMoving = false;
                _waitTimer = _waitTimeAtWaypoint;
            }
        }

        private void HandleWaitTimer()
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                // Pick next waypoint (looping)
                _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Count;
                _isMoving = true;
            }
        }

        /// <summary>
        /// Used by the Boss Map configurator to assign waypoints dynamically
        /// </summary>
        public void SetWaypoints(List<Vector3> waypoints)
        {
            _waypoints = waypoints;
            if (_waypoints.Count > 0)
            {
                transform.position = _waypoints[0];
                _currentWaypointIndex = 0;
            }
        }
    }
}
