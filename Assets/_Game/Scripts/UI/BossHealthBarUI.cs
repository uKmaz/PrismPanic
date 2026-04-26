using UnityEngine;
using UnityEngine.UI;
using PrismPanic.Core;
using PrismPanic.Enemies;

namespace PrismPanic.UI
{
    /// <summary>
    /// Dark Souls-style boss health bar at the bottom of the screen.
    /// Shows boss name + wide health bar with smooth damage lerp.
    /// Auto-shows when boss is active, auto-hides when boss dies or room changes.
    /// Uses CanvasGroup for visibility so the script stays alive to receive events.
    /// </summary>
    public class BossHealthBarUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup _canvasGroup;      // Controls visibility (add CanvasGroup component)
        [SerializeField] private Image _healthFill;             // The red/colored fill bar
        [SerializeField] private Image _damageFill;             // Delayed yellow "damage taken" bar
        [SerializeField] private TMPro.TextMeshProUGUI _bossNameText;

        [Header("Settings")]
        [SerializeField] private string _bossName = "PRISM GUARDIAN";
        [SerializeField] private float _damageLerpSpeed = 1.5f;
        [SerializeField] private float _damageLerpDelay = 0.6f;

        private BossController _boss;
        private float _targetFill;
        private float _damageFillAmount;
        private float _damageDelayTimer;
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
            _targetFill = 1f;
            _damageFillAmount = 1f;
            _damageDelayTimer = 0f;

            SetBarVisible(true);

            if (_bossNameText != null)
                _bossNameText.text = _bossName;

            if (_healthFill != null)
                _healthFill.fillAmount = 1f;

            if (_damageFill != null)
                _damageFill.fillAmount = 1f;
        }

        private void HideBar()
        {
            _isVisible = false;
            _boss = null;
            SetBarVisible(false);
        }

        private void SetBarVisible(bool visible)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = visible ? 1f : 0f;
                _canvasGroup.blocksRaycasts = visible;
                _canvasGroup.interactable = visible;
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

            float currentFill = (float)_boss.CurrentHealth / _boss.MaxHealth;
            currentFill = Mathf.Clamp01(currentFill);

            _targetFill = currentFill;
            if (_healthFill != null)
                _healthFill.fillAmount = _targetFill;

            if (_damageFill != null)
            {
                if (_damageFillAmount > _targetFill + 0.001f)
                {
                    if (_damageDelayTimer > 0f)
                    {
                        _damageDelayTimer -= Time.deltaTime;
                    }
                    else
                    {
                        _damageFillAmount = Mathf.MoveTowards(
                            _damageFillAmount, _targetFill, _damageLerpSpeed * Time.deltaTime);
                    }
                }
                else
                {
                    _damageFillAmount = _targetFill;
                    _damageDelayTimer = _damageLerpDelay;
                }

                _damageFill.fillAmount = _damageFillAmount;
            }
        }
    }
}
