using UnityEngine;
using TMPro;
using PrismPanic.Core;

namespace PrismPanic.UI
{
    /// <summary>
    /// HUD showing angel count and current level. Top-left corner, minimal.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text _angelCountText;
        [SerializeField] private TMP_Text _levelText;

        private void OnEnable()
        {
            EventBus.OnAngelKilled += HandleAngelKilled;
            EventBus.OnLevelStart += HandleLevelStart;
        }

        private void OnDisable()
        {
            EventBus.OnAngelKilled -= HandleAngelKilled;
            EventBus.OnLevelStart -= HandleLevelStart;
        }

        private void HandleAngelKilled(GameObject angel)
        {
            UpdateAngelCount();
        }

        private void HandleLevelStart()
        {
            UpdateAngelCount();
            UpdateLevel();
        }

        private void UpdateAngelCount()
        {
            if (_angelCountText == null || GameManager.Instance == null) return;
            _angelCountText.text = $"Angels: {GameManager.Instance.ActiveAngelCount}";
        }

        private void UpdateLevel()
        {
            if (_levelText == null || GameManager.Instance == null) return;
            _levelText.text = $"Level {GameManager.Instance.CurrentLevelIndex + 1}";
        }
    }
}
