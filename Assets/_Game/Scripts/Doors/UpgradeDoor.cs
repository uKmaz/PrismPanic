using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using PrismPanic.Core;
using PrismPanic.ScriptableObjects;

namespace PrismPanic.Doors
{
    /// <summary>
    /// Individual upgrade door. Shows upgrade info and triggers upgrade on F key press.
    /// Uses distance check for reliable CharacterController detection.
    /// Icon is displayed via a child SpriteRenderer — no Canvas needed.
    /// </summary>
    public class UpgradeDoor : MonoBehaviour
    {
        [Header("Icon (SpriteRenderer child)")]
        [SerializeField] private SpriteRenderer _iconRenderer;

        [Header("Name Label (optional TMP in world space)")]
        [SerializeField] private TMP_Text _nameLabel;

        [Header("Prompt")]
        [SerializeField] private GameObject _promptUI;        // "F" icon/text — child GO, toggled on/off
        [SerializeField] private SpriteRenderer _doorSprite;  // Door SpriteRenderer for open/closed swap

        [Header("Door Sprites")]
        [SerializeField] private Sprite _closedSprite;
        [SerializeField] private Sprite _openSprite;

        [Header("Settings")]
        // Radii are driven by Constants — edit them there, not per-instance
        private readonly float _interactRadius = Constants.DOOR_INTERACT_RADIUS;
        private readonly float _confirmRadius  = Constants.DOOR_CONFIRM_RADIUS;

        private UpgradeDefinitionSO _upgrade;
        private bool _triggered;
        private bool _playerInRange;
        private Transform _playerTransform;

        public void Setup(UpgradeDefinitionSO upgrade)
        {
            _upgrade = upgrade;
            _triggered = false;
            _playerInRange = false;

            // Assign icon sprite directly to SpriteRenderer
            if (_iconRenderer != null)
            {
                _iconRenderer.sprite = upgrade.icon; // null is fine — hides icon if no sprite assigned
                _iconRenderer.gameObject.SetActive(upgrade.icon != null);
            }

            if (_nameLabel != null)
                _nameLabel.text = upgrade.displayName;

            // Reset door visual
            if (_doorSprite != null && _closedSprite != null)
                _doorSprite.sprite = _closedSprite;

            if (_promptUI != null)
                _promptUI.SetActive(false);

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

            bool wasInRange = _playerInRange;
            _playerInRange = dist <= _interactRadius;

            // --- Enter range: show prompt, open door sprite, hide icon ---
            if (_playerInRange && !wasInRange)
            {
                if (_promptUI != null)
                    _promptUI.SetActive(true);

                if (_doorSprite != null && _openSprite != null)
                    _doorSprite.sprite = _openSprite;

                if (_iconRenderer != null)
                    _iconRenderer.gameObject.SetActive(false);
            }
            // --- Exit range: hide prompt, close door sprite, show icon ---
            else if (!_playerInRange && wasInRange)
            {
                if (_promptUI != null)
                    _promptUI.SetActive(false);

                if (_doorSprite != null && _closedSprite != null)
                    _doorSprite.sprite = _closedSprite;

                if (_iconRenderer != null)
                    _iconRenderer.gameObject.SetActive(true);
            }

            // --- F key to confirm ---
            if (_playerInRange && dist <= _confirmRadius)
            {
                var kb = Keyboard.current;
                if (kb != null && kb.fKey.wasPressedThisFrame)
                {
                    _triggered = true;

                    if (_promptUI != null)
                        _promptUI.SetActive(false);

                    Debug.Log($"[UpgradeDoor] Player pressed F ({dist:F1}m)! Granting: {_upgrade.displayName}");
                    EventBus.FireUpgradeSelected(_upgrade);
                }
            }
        }

        private void OnDisable()
        {
            // Reset state when returned to pool
            _playerInRange = false;

            if (_promptUI != null)
                _promptUI.SetActive(false);

            if (_doorSprite != null && _closedSprite != null)
                _doorSprite.sprite = _closedSprite;
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize interact radius in Scene view
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactRadius);
        }
    }
}
