using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using PrismPanic.Core;
using PrismPanic.ScriptableObjects;
using PrismPanic.Utilities;

namespace PrismPanic.Room
{
    /// <summary>
    /// Rebuilds the room from a LevelLayoutSO. Listens to OnRoomReconfigure.
    /// Pools all environment objects — never Instantiate/Destroy in hot paths.
    /// </summary>
    public class RoomConfigurator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private NavMeshSurface _navMeshSurface;

        // Track active pooled objects for return
        private readonly List<Transform> _activeWalls = new List<Transform>();
        private readonly List<Transform> _activeFloors = new List<Transform>();
        private readonly List<Transform> _activeMirrors = new List<Transform>();
        private readonly List<Transform> _activePillars = new List<Transform>();
        private readonly List<Transform> _activeEnemies = new List<Transform>();

        private LevelLayoutSO _currentLayout;

        private void OnEnable()
        {
            EventBus.OnRoomReconfigure += HandleReconfigure;
        }

        private void OnDisable()
        {
            EventBus.OnRoomReconfigure -= HandleReconfigure;
        }

        private void HandleReconfigure(ScriptableObject layoutSO)
        {
            _currentLayout = layoutSO as LevelLayoutSO;
            if (_currentLayout == null) return;

            ClearRoom();
            BuildRoom(_currentLayout);
            SpawnEnemies(_currentLayout);
            MovePlayer(_currentLayout.playerSpawnPoint);
            RebakeNavMesh();

            EventBus.FireRoomReconfigureComplete();
        }

        private void ClearRoom()
        {
            var pool = PoolManager.Instance;
            if (pool == null) return;

            pool.Walls?.ReturnAll(_activeWalls);
            pool.Floors?.ReturnAll(_activeFloors);
            pool.Mirrors?.ReturnAll(_activeMirrors);
            pool.Pillars?.ReturnAll(_activePillars);

            // Return enemies
            if (pool.Angels != null)
            {
                pool.Angels.ReturnAll(_activeEnemies);
            }
        }

        private void BuildRoom(LevelLayoutSO layout)
        {
            var pool = PoolManager.Instance;
            if (pool == null) return;

            // Place walls
            if (layout.wallPositions != null && pool.Walls != null)
            {
                foreach (Vector3 pos in layout.wallPositions)
                {
                    Transform wall = pool.Walls.Get();
                    wall.position = pos;
                    wall.rotation = Quaternion.identity;
                    _activeWalls.Add(wall);
                }
            }

            // Place floors
            if (layout.floorPositions != null && pool.Floors != null)
            {
                foreach (Vector3 pos in layout.floorPositions)
                {
                    Transform floor = pool.Floors.Get();
                    floor.position = pos;
                    floor.rotation = Quaternion.identity;
                    _activeFloors.Add(floor);
                }
            }

            // Place mirrors
            if (layout.mirrorPlacements != null && pool.Mirrors != null)
            {
                foreach (MirrorPlacement mp in layout.mirrorPlacements)
                {
                    Transform mirror = pool.Mirrors.Get();
                    mirror.position = mp.position;
                    mirror.rotation = Quaternion.Euler(0f, mp.rotationY, 0f);
                    _activeMirrors.Add(mirror);
                }
            }

            // Place pillars
            if (layout.pillarPositions != null && pool.Pillars != null)
            {
                foreach (Vector3 pos in layout.pillarPositions)
                {
                    Transform pillar = pool.Pillars.Get();
                    pillar.position = pos;
                    pillar.rotation = Quaternion.identity;
                    _activePillars.Add(pillar);
                }
            }
        }

        private void SpawnEnemies(LevelLayoutSO layout)
        {
            var pool = PoolManager.Instance;
            if (pool?.Angels == null || layout.waves == null) return;

            int spawnIndex = 0;
            int totalAngels = 0;

            foreach (EnemyWaveData wave in layout.waves)
            {
                for (int i = 0; i < wave.count; i++)
                {
                    Transform angel = pool.Angels.Get();
                    Vector3 spawnPos = layout.enemySpawnPoints != null && spawnIndex < layout.enemySpawnPoints.Length
                        ? layout.enemySpawnPoints[spawnIndex % layout.enemySpawnPoints.Length]
                        : Vector3.zero;

                    angel.position = spawnPos;

                    // Initialize angel with data
                    var controller = angel.GetComponent<Enemies.AngelController>();
                    if (controller != null)
                    {
                        controller.Initialize(wave.enemyData);
                    }

                    _activeEnemies.Add(angel);
                    spawnIndex++;
                    totalAngels++;
                }
            }

            // Tell GameManager how many angels are active
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetAngelCount(totalAngels);
            }
        }

        private void MovePlayer(Vector3 spawnPoint)
        {
            if (_playerTransform != null)
            {
                _playerTransform.position = spawnPoint;
            }
        }

        private void RebakeNavMesh()
        {
            if (_navMeshSurface != null)
            {
                _navMeshSurface.BuildNavMesh();
            }
        }

        /// <summary>
        /// Called by FlashlightController to place a player-placed mirror.
        /// </summary>
        public void PlaceExtraMirror(Vector3 position, float rotationY)
        {
            var pool = PoolManager.Instance;
            if (pool?.Mirrors == null) return;

            Transform mirror = pool.Mirrors.Get();
            mirror.position = position;
            mirror.rotation = Quaternion.Euler(0f, rotationY, 0f);
            _activeMirrors.Add(mirror);
        }

        public LevelLayoutSO CurrentLayout => _currentLayout;
    }
}
