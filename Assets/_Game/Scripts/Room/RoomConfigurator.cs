using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using PrismPanic.Core;
using PrismPanic.ScriptableObjects;
using PrismPanic.Utilities;
using PrismPanic.BOSS;

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

        [Header("Boss")]
        [SerializeField] private GameObject _bossPrefab;
        [SerializeField] private BOSS.BossRoomManager _bossRoomManager;

        private GameObject _activeBoss;

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
            MovePlayer(_currentLayout.playerSpawnPoint);
            RebakeNavMesh();

            if (_currentLayout.hasBoss)
            {
                SpawnBoss(_currentLayout);
            }
            else
            {
                SpawnEnemies(_currentLayout);
            }

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

            // Deactivate boss if present
            if (_activeBoss != null)
                _activeBoss.SetActive(false);

            // Return enemies to their correct pools
            foreach (var enemy in _activeEnemies)
            {
                if (enemy == null) continue;
                var angel = enemy.GetComponent<Enemies.AngelController>();
                // Check if it's a shadow angel — if so, return to shadow pool
                bool isShadow = angel != null && angel.IsInvisibleType;
                var targetPool = isShadow ? pool.ShadowAngels : pool.Angels;
                targetPool?.Return(enemy);
            }
            _activeEnemies.Clear();
        }

        private void BuildRoom(LevelLayoutSO layout)
        {
            var pool = PoolManager.Instance;
            if (pool == null) return;

            // Place walls
            if (layout.wallPositions != null && pool.Walls != null)
            {
                // First pass: find room boundaries
                float minX = float.MaxValue, maxX = float.MinValue;
                float minZ = float.MaxValue, maxZ = float.MinValue;

                foreach (Vector3 pos in layout.wallPositions)
                {
                    if (pos.x < minX) minX = pos.x;
                    if (pos.x > maxX) maxX = pos.x;
                    if (pos.z < minZ) minZ = pos.z;
                    if (pos.z > maxZ) maxZ = pos.z;
                }

                // Second pass: spawn and configure
                foreach (Vector3 pos in layout.wallPositions)
                {
                    Transform wall = pool.Walls.Get();
                    wall.position = pos;
                    wall.rotation = Quaternion.identity;
                    _activeWalls.Add(wall);

                    // Configure sprite appearance based on position
                    WallAppearance appearance = wall.GetComponent<WallAppearance>();
                    if (appearance != null)
                    {
                        // Use small tolerance for float comparison
                        bool isLeft = Mathf.Abs(pos.x - minX) < 0.1f;
                        bool isRight = Mathf.Abs(pos.x - maxX) < 0.1f;
                        bool isBottom = Mathf.Abs(pos.z - minZ) < 0.1f;
                        bool isTop = Mathf.Abs(pos.z - maxZ) < 0.1f;

                        appearance.Setup(isTop, isBottom, isLeft, isRight);
                    }
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
            if (pool == null || layout.waves == null) return;

            int spawnIndex = 0;
            int totalAngels = 0;

            foreach (EnemyWaveData wave in layout.waves)
            {
                // Pick pool based on angel type
                var angelPool = wave.isShadowAngel ? pool.ShadowAngels : pool.Angels;
                if (angelPool == null)
                {
                    Debug.LogWarning($"[RoomConfigurator] {(wave.isShadowAngel ? "ShadowAngels" : "Angels")} pool is null! Skipping wave.");
                    continue;
                }

                for (int i = 0; i < wave.count; i++)
                {
                    Transform angel = angelPool.Get();
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
            // If reference is missing or doesn't have CC, find the true Player object dynamically!
            if (_playerTransform == null || _playerTransform.GetComponent<CharacterController>() == null)
            {
                var playerObj = Object.FindObjectOfType<PrismPanic.Player.PlayerController>();
                if (playerObj != null)
                {
                    _playerTransform = playerObj.transform;
                    Debug.LogWarning($"[DEEP LOG] _playerTransform was invalid. Dynamically found true Player at: {_playerTransform.name}");
                }
            }

            if (_playerTransform != null)
            {
                var cc = _playerTransform.GetComponent<CharacterController>();
                
                Debug.Log($"[DEEP LOG] MovePlayer STARTED. Frame: {Time.frameCount}. Current Player Pos: {_playerTransform.position}. Target Spawn: {spawnPoint}. CC Enabled: {(cc != null ? cc.enabled.ToString() : "NULL")}");

                if (cc != null) cc.enabled = false;
                
                // Force spawn slightly higher so player doesn't clip into the floor on teleport
                spawnPoint.y = 1.5f;
                _playerTransform.position = spawnPoint;
                
                if (cc != null) cc.enabled = true;

                Debug.Log($"[DEEP LOG] MovePlayer COMPLETED. Frame: {Time.frameCount}. New Player Pos: {_playerTransform.position}. CC Enabled: {(cc != null ? cc.enabled.ToString() : "NULL")}");
            }
            else
            {
                Debug.LogError("[DEEP LOG] MovePlayer FAILED: _playerTransform is absolutely NULL and couldn't find PlayerController!");
            }
        }

        private void RebakeNavMesh()
        {
            if (_navMeshSurface == null)
            {
                Debug.LogError("[RoomConfigurator] _navMeshSurface is NULL! Assign NavMeshSurface in Inspector. Angels won't move.");
                return;
            }

            _navMeshSurface.BuildNavMesh();
            Debug.Log("[RoomConfigurator] NavMesh baked.");
        }

        /// <summary>
        /// Called by FlashlightController to place a player-placed mirror.
        /// </summary>
        public void PlaceExtraMirror(Vector3 position, Quaternion rotation)
        {
            var pool = PoolManager.Instance;
            if (pool?.Mirrors == null) return;

            Transform mirror = pool.Mirrors.Get();
            mirror.position = position;
            mirror.rotation = rotation;
            _activeMirrors.Add(mirror);

            // Rebake NavMesh so angels path around the new mirror
            RebakeNavMesh();
        }

        public LevelLayoutSO CurrentLayout => _currentLayout;

        private void SpawnBoss(LevelLayoutSO layout)
        {
            // Deactivate any old boss
            if (_activeBoss != null)
            {
                _activeBoss.SetActive(false);
            }

            if (_bossPrefab != null)
            {
                if (_activeBoss == null)
                {
                    _activeBoss = Instantiate(_bossPrefab);
                }
                _activeBoss.transform.position = layout.bossSpawnPoint;
                _activeBoss.transform.rotation = Quaternion.identity;
                _activeBoss.SetActive(true);
            }

            // Initialize BossRoomManager for dynamic pillar/mirror shuffling
            if (_bossRoomManager != null)
            {
                // Compute room bounds from wall positions
                float minX = float.MaxValue, maxX = float.MinValue;
                float minZ = float.MaxValue, maxZ = float.MinValue;

                if (layout.wallPositions != null)
                {
                    foreach (Vector3 pos in layout.wallPositions)
                    {
                        if (pos.x < minX) minX = pos.x;
                        if (pos.x > maxX) maxX = pos.x;
                        if (pos.z < minZ) minZ = pos.z;
                        if (pos.z > maxZ) maxZ = pos.z;
                    }
                }

                _bossRoomManager.Initialize(
                    _activePillars,
                    _activeMirrors,
                    layout.bossSpawnPoint,
                    minX, maxX, minZ, maxZ,
                    _navMeshSurface
                );
            }
        }
    }
}
