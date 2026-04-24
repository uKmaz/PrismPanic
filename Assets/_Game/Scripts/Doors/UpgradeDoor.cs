using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PrismPanic.Core;
using PrismPanic.ScriptableObjects;

namespace PrismPanic.Doors
{
    /// <summary>
    /// Individual upgrade door. Shows upgrade info and triggers upgrade on player entry.
    /// Has a world-space canvas child displaying icon + name.
    /// </summary>
    public class UpgradeDoor : MonoBehaviour
    {
        [Header("UI Elements (World Space Canvas child)")]
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private Image _iconImage;

        private UpgradeDefinitionSO _upgrade;
        private bool _triggered;

        public void Setup(UpgradeDefinitionSO upgrade)
        {
            _upgrade = upgrade;
            _triggered = false;

            if (_nameLabel != null)
                _nameLabel.text = upgrade.displayName;

            if (_iconImage != null && upgrade.icon != null)
                _iconImage.sprite = upgrade.icon;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered) return;
            if (other.gameObject.layer != Constants.LayerPlayer) return;
            if (_upgrade == null) return;

            _triggered = true;
            EventBus.FireUpgradeSelected(_upgrade);
        }
    }
}
