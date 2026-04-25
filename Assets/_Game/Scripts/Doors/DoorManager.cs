using System.Collections.Generic;
using UnityEngine;
using PrismPanic.Core;
using PrismPanic.ScriptableObjects;
using PrismPanic.Utilities;

namespace PrismPanic.Doors
{
    /// <summary>
    /// Activates 3 upgrade doors when all angels are cleared.
    /// Assigns random non-duplicate upgrades from the registry.
    /// </summary>
    public class DoorManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Upgrades.UpgradeRegistry _upgradeRegistry;

        private readonly List<Transform> _activeDoors = new List<Transform>();
        private readonly HashSet<string> _usedUpgradeIDs = new HashSet<string>();
        private LevelLayoutSO _currentLayout;

        private void OnEnable()
        {
            EventBus.OnDoorsOpen += HandleDoorsOpen;
            EventBus.OnUpgradeSelected += HandleUpgradeSelected;
            EventBus.OnRoomReconfigure += HandleRoomReconfigure;
            EventBus.OnGameRestart += HandleRestart;
        }

        private void OnDisable()
        {
            EventBus.OnDoorsOpen -= HandleDoorsOpen;
            EventBus.OnUpgradeSelected -= HandleUpgradeSelected;
            EventBus.OnRoomReconfigure -= HandleRoomReconfigure;
            EventBus.OnGameRestart -= HandleRestart;
        }

        private void HandleRoomReconfigure(ScriptableObject layoutSO)
        {
            _currentLayout = layoutSO as LevelLayoutSO;
            Debug.Log($"[DoorManager] Layout received: {(_currentLayout != null ? _currentLayout.layoutID : "NULL")}");
            CloseDoors();
        }

        private void HandleDoorsOpen()
        {
            Debug.Log($"[DoorManager] DoorsOpen fired! Layout={_currentLayout != null}, Registry={_upgradeRegistry != null}");

            if (_currentLayout == null)
            {
                Debug.LogError("[DoorManager] _currentLayout is NULL! Was OnRoomReconfigure received?");
                return;
            }

            if (_upgradeRegistry == null)
            {
                Debug.LogError("[DoorManager] _upgradeRegistry is NULL! Assign it in Inspector.");
                return;
            }

            var pool = PoolManager.Instance;
            if (pool == null)
            {
                Debug.LogError("[DoorManager] PoolManager.Instance is NULL!");
                return;
            }

            if (pool.Doors == null)
            {
                Debug.LogError("[DoorManager] pool.Doors is NULL! Assign door prefab in PoolManager.");
                return;
            }

            List<UpgradeDefinitionSO> upgrades = _upgradeRegistry.GetRandomUpgrades(
                Constants.DOORS_PER_LEVEL, _usedUpgradeIDs
            );

            Debug.Log($"[DoorManager] Got {upgrades.Count} upgrades. Spawning doors...");

            if (upgrades.Count == 0)
            {
                Debug.LogWarning("[DoorManager] No upgrades available! All used up. Skipping to next level.");
                // Auto-advance if no upgrades left
                EventBus.FireUpgradeSelected(null);
                return;
            }

            for (int i = 0; i < Constants.DOORS_PER_LEVEL && i < upgrades.Count; i++)
            {
                Transform doorTransform = pool.Doors.Get();

                Vector3 spawnPos = _currentLayout.doorSpawnPoints != null && i < _currentLayout.doorSpawnPoints.Length
                    ? _currentLayout.doorSpawnPoints[i]
                    : Vector3.zero;

                doorTransform.position = spawnPos;

                UpgradeDoor door = doorTransform.GetComponent<UpgradeDoor>();
                if (door != null)
                {
                    door.Setup(upgrades[i]);
                }

                _activeDoors.Add(doorTransform);
                Debug.Log($"[DoorManager] Door {i} spawned at {spawnPos} with upgrade: {upgrades[i].displayName}");
            }
        }

        private void HandleUpgradeSelected(ScriptableObject upgradeSO)
        {
            var upgrade = upgradeSO as UpgradeDefinitionSO;
            if (upgrade != null)
            {
                _usedUpgradeIDs.Add(upgrade.upgradeID);
            }

            CloseDoors();
        }

        private void CloseDoors()
        {
            var pool = PoolManager.Instance;
            if (pool?.Doors == null) return;

            pool.Doors.ReturnAll(_activeDoors);
        }

        private void HandleRestart()
        {
            _usedUpgradeIDs.Clear();
            CloseDoors();
        }
    }
}
