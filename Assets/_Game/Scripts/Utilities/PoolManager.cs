using UnityEngine;
using PrismPanic.Core;

namespace PrismPanic.Utilities
{
    /// <summary>
    /// Singleton that owns all object pools. Lives on a persistent GameObject.
    /// Assign prefabs and containers via Inspector.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private Transform _angelPrefab;
        [SerializeField] private Transform _shadowAngelPrefab;
        [SerializeField] private Transform _beamSegmentPrefab;
        [SerializeField] private Transform _mirrorPrefab;
        [SerializeField] private Transform _pillarPrefab;
        [SerializeField] private Transform _doorPrefab;
        [SerializeField] private Transform _wallPrefab;
        [SerializeField] private Transform _floorPrefab;

        [Header("Containers (parent transforms in scene hierarchy)")]
        [SerializeField] private Transform _enemiesContainer;
        [SerializeField] private Transform _beamContainer;
        [SerializeField] private Transform _mirrorsContainer;
        [SerializeField] private Transform _pillarsContainer;
        [SerializeField] private Transform _doorsContainer;
        [SerializeField] private Transform _wallsContainer;
        [SerializeField] private Transform _floorsContainer;

        // Public pool accessors
        public ObjectPool<Transform> Angels { get; private set; }
        public ObjectPool<Transform> ShadowAngels { get; private set; }
        public ObjectPool<Transform> BeamSegments { get; private set; }
        public ObjectPool<Transform> Mirrors { get; private set; }
        public ObjectPool<Transform> Pillars { get; private set; }
        public ObjectPool<Transform> Doors { get; private set; }
        public ObjectPool<Transform> Walls { get; private set; }
        public ObjectPool<Transform> Floors { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializePools();
        }

        private void InitializePools()
        {
            if (_angelPrefab != null)
                Angels = new ObjectPool<Transform>(_angelPrefab, _enemiesContainer, Constants.POOL_ANGELS);

            if (_shadowAngelPrefab != null)
                ShadowAngels = new ObjectPool<Transform>(_shadowAngelPrefab, _enemiesContainer, Constants.POOL_SHADOW_ANGELS);

            if (_beamSegmentPrefab != null)
                BeamSegments = new ObjectPool<Transform>(_beamSegmentPrefab, _beamContainer, Constants.POOL_BEAM_SEGMENTS);

            if (_mirrorPrefab != null)
                Mirrors = new ObjectPool<Transform>(_mirrorPrefab, _mirrorsContainer, Constants.POOL_MIRRORS);

            if (_pillarPrefab != null)
                Pillars = new ObjectPool<Transform>(_pillarPrefab, _pillarsContainer, Constants.POOL_PILLARS);

            if (_doorPrefab != null)
                Doors = new ObjectPool<Transform>(_doorPrefab, _doorsContainer, Constants.POOL_DOORS);

            if (_wallPrefab != null)
                Walls = new ObjectPool<Transform>(_wallPrefab, _wallsContainer, Constants.POOL_WALLS);

            if (_floorPrefab != null)
                Floors = new ObjectPool<Transform>(_floorPrefab, _floorsContainer, Constants.POOL_FLOORS);
        }
    }
}
