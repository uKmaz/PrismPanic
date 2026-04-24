using UnityEngine;
using TMPro;
using UnityEngine.UI;
using PrismPanic.Core;
using PrismPanic.ScriptableObjects;

namespace PrismPanic.Doors
{
    /// <summary>
    /// Individual upgrade door. Shows upgrade info and triggers upgrade on player proximity.
    /// Uses distance check instead of OnTriggerEnter for reliable CharacterController detection.
    /// </summary>
    public class UpgradeDoor : MonoBehaviour
    {
        [Header("UI Elements (World Space Canvas child)")]
        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private Image _iconImage;

        [Header("Settings")]
        [SerializeField] private float _interactRadius = 0.6f;

        private UpgradeDefinitionSO _upgrade;
        private bool _triggered;
        private Transform _playerTransform;

        public void Setup(UpgradeDefinitionSO upgrade)
        {
            _upgrade = upgrade;
            _triggered = false;

            if (_nameLabel != null)
                _nameLabel.text = upgrade.displayName;

            if (_iconImage != null && upgrade.icon != null)
                _iconImage.sprite = upgrade.icon;

            // Cache player
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                _playerTransform = player.transform;
        }

        private void Update()
        {
            if (_triggered || _upgrade == null || _playerTransform == null) return;

            // Only check during DoorsOpen phase
            if (GameManager.Instance == null || GameManager.Instance.CurrentPhase != GamePhase.DoorsOpen)
                return;

            float dist = Vector3.Distance(
                new Vector3(transform.position.x, 0f, transform.position.z),
                new Vector3(_playerTransform.position.x, 0f, _playerTransform.position.z)
            );

            if (dist <= _interactRadius)
            {
                Debug.Log($"[UpgradeDoor] Player in range ({dist:F1}m)! Granting: {_upgrade.displayName}");
                _triggered = true;
                EventBus.FireUpgradeSelected(_upgrade);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize interact radius in Scene view
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactRadius);
        }
    }
}
