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
<<<<<<< Updated upstream
        [SerializeField] private GameObject _shieldUI; // Drag the ShieldUI GameObject here
=======
        [SerializeField] private GameObject _shieldUI;
>>>>>>> Stashed changes

        private void OnEnable()
        {
            EventBus.OnUpgradeSelected += HandleUpgradeSelected;
            EventBus.OnGameRestart += HandleRestart;
        }

        private void OnDisable()
        {
            EventBus.OnUpgradeSelected -= HandleUpgradeSelected;
            EventBus.OnGameRestart -= HandleRestart;
        }

        private void HandleRestart()
        {
            if (_shieldUI != null) _shieldUI.SetActive(false);
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

                case UpgradeEffectType.BeamRadiusWidenerBonus:
                    _playerStats.beamRadius += upgrade.effectValue;
                    break;

                case UpgradeEffectType.ExtraMirrorPlacement:
                    _playerStats.hasShield = true;
<<<<<<< Updated upstream
                    // Enable ShieldUI GameObject
                    if (_shieldUI != null) _shieldUI.SetActive(true);
                    // Auto-activate shield
=======
                    if (_shieldUI != null) _shieldUI.SetActive(true);
>>>>>>> Stashed changes
                    var flashlight = FindObjectOfType<Player.FlashlightController>();
                    if (flashlight != null) flashlight.ActivateShield();
                    break;

                case UpgradeEffectType.BeamColorChangeBonus:
                    _playerStats.hasBlueBeam = true;
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
