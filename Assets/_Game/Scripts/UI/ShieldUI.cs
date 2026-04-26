using UnityEngine;
using UnityEngine.UI;
using PrismPanic.ScriptableObjects;

namespace PrismPanic.UI
{
    /// <summary>
    /// Shows shield on/off icon below the energy bar.
    /// Swaps between two sprites based on whether the shield mirror is deployed.
    /// GameObject starts disabled — UpgradeApplier enables it when shield upgrade is picked.
    /// </summary>
    public class ShieldUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerStatsSO _playerStats;
        [SerializeField] private Image _shieldIcon;

        [Header("Sprites")]
        [SerializeField] private Sprite _shieldOnSprite;
        [SerializeField] private Sprite _shieldOffSprite;

        private Player.FlashlightController _flashlightController;

        private void Update()
        {
            if (_playerStats == null || _shieldIcon == null) return;
            if (!_playerStats.hasShield) return;

            // Cache FlashlightController reference
            if (_flashlightController == null)
                _flashlightController = FindObjectOfType<Player.FlashlightController>();

            if (_flashlightController == null) return;

            // Swap sprite based on shield state
            _shieldIcon.sprite = _flashlightController.IsShieldActive
                ? _shieldOnSprite
                : _shieldOffSprite;
        }
    }
}
