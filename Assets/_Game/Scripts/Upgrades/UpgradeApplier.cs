using UnityEngine;
using PrismPanic.Core;
using PrismPanic.ScriptableObjects;

namespace PrismPanic.Upgrades
{
    /// <summary>
    /// Applies upgrade effects to PlayerStatsSO when an upgrade is selected.
    /// </summary>
    public class UpgradeApplier : MonoBehaviour
    {
        [SerializeField] private PlayerStatsSO _playerStats;

        private void OnEnable()
        {
            EventBus.OnUpgradeSelected += HandleUpgradeSelected;
        }

        private void OnDisable()
        {
            EventBus.OnUpgradeSelected -= HandleUpgradeSelected;
        }

        private void HandleUpgradeSelected(ScriptableObject upgradeSO)
        {
            var upgrade = upgradeSO as UpgradeDefinitionSO;
            if (upgrade == null || _playerStats == null) return;

            switch (upgrade.effectType)
            {
                case UpgradeEffectType.StunDurationBonus:
                    _playerStats.stunDuration += upgrade.effectValue;
                    break;

                case UpgradeEffectType.MovementSpeedBonus:
                    _playerStats.moveSpeed += upgrade.effectValue;
                    break;

                case UpgradeEffectType.FlashlightConeWidth:
                    _playerStats.wideAngle *= 1.30f;
                    break;

                case UpgradeEffectType.ExtraMirrorPlacement:
                    _playerStats.mirrorsPerRoom += (int)upgrade.effectValue;
                    // Also top up the current room's budget immediately
                    _playerStats.placeableMirrorCount = _playerStats.mirrorsPerRoom;
                    break;

                case UpgradeEffectType.BeamRangeBonus:
                    _playerStats.beamRange += upgrade.effectValue;
                    _playerStats.beamGrowthSpeed *= 0.5f; 
                    break;
                    
                case UpgradeEffectType.Multishot:
                    _playerStats.multishotCount = 2;
                    _playerStats.energyDrainMultiplier *= 1.5f; 
                    break;

                case UpgradeEffectType.ExtraEnergy:
                    _playerStats.maxEnergy += upgrade.effectValue;
                    _playerStats.currentEnergy += upgrade.effectValue;
                    break;
            }
        }
    }
}
