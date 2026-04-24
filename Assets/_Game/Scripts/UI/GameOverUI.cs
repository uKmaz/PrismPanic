using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PrismPanic.Core;

namespace PrismPanic.UI
{
    /// <summary>
    /// Game Over screen. Shows on player death with level reached and restart button.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _levelReachedText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;

        private void Awake()
        {
            if (_panel != null)
                _panel.SetActive(false);

            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestart);

            if (_menuButton != null)
                _menuButton.onClick.AddListener(OnMenu);
        }

        private void OnEnable()
        {
            EventBus.OnPlayerDeath += HandlePlayerDeath;
        }

        private void OnDisable()
        {
            EventBus.OnPlayerDeath -= HandlePlayerDeath;
        }

        private void HandlePlayerDeath()
        {
            if (_panel != null)
                _panel.SetActive(true);

            if (_levelReachedText != null && GameManager.Instance != null)
                _levelReachedText.text = $"You reached Level {GameManager.Instance.CurrentLevelIndex + 1}";

            // Pause game
            Time.timeScale = 0f;
        }

        private void OnRestart()
        {
            Time.timeScale = 1f;
            EventBus.FireGameRestart();
        }

        private void OnMenu()
        {
            Time.timeScale = 1f;
            SceneController.LoadMenu();
        }
    }
}
