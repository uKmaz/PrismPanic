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
            CloseDoors();
        }

        private void HandleDoorsOpen()
        {
            if (_currentLayout == null || _upgradeRegistry == null) return;

            var pool = PoolManager.Instance;
            if (pool?.Doors == null) return;

            List<UpgradeDefinitionSO> upgrades = _upgradeRegistry.GetRandomUpgrades(
                Constants.DOORS_PER_LEVEL, _usedUpgradeIDs
            );

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
