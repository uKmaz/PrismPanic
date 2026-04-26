using UnityEngine;
using UnityEngine.UI;
using PrismPanic.Core;
using PrismPanic.Enemies;

namespace PrismPanic.UI
{
    /// <summary>
    /// Classic boss health bar at the top or bottom of the screen.
    /// Updates a standard UI Image (Image Type: Filled).
    /// </summary>
    public class BossHealthBarUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _healthFill;
        [SerializeField] private TMPro.TextMeshProUGUI _bossNameText;

        [Header("Settings")]
        [SerializeField] private string _bossName = "PRISM GUARDIAN";

        private BossController _boss;
        private bool _isVisible;

        private void Start()
        {
            HideBar();
        }

        private void OnEnable()
        {
            EventBus.OnRoomReconfigureComplete += FindBoss;
            EventBus.OnVictory += HideBar;
        }

        private void OnDisable()
        {
            EventBus.OnRoomReconfigureComplete -= FindBoss;
            EventBus.OnVictory -= HideBar;
        }

        private void FindBoss()
        {
            _boss = FindObjectOfType<BossController>();
            if (_boss != null && _boss.gameObject.activeInHierarchy)
            {
                ShowBar();
            }
            else
            {
                HideBar();
            }
        }

        private void ShowBar()
        {
            _isVisible = true;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = true;
            }

            if (_bossNameText != null)
                _bossNameText.text = _bossName;

            if (_healthFill != null)
                _healthFill.fillAmount = 1f;
        }

        private void HideBar()
        {
            _isVisible = false;
            _boss = null;
            
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }
        }

        private void Update()
        {
            if (!_isVisible || _boss == null) return;

            if (!_boss.gameObject.activeInHierarchy)
            {
                HideBar();
                return;
            }

            // Update health fill
            if (_healthFill != null)
            {
                float currentFill = (float)_boss.CurrentHealth / _boss.MaxHealth;
                _healthFill.fillAmount = Mathf.Clamp01(currentFill);
            }
        }
    }
}
