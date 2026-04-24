using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PrismPanic.Core;

namespace PrismPanic.UI
{
    /// <summary>
    /// Game Over screen. MUST be on an ALWAYS-ACTIVE GameObject.
    /// The _panel child gets shown/hidden — not this component's GO.
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
            // Hide panel on start — this GO must stay active!
            if (_panel != null)
                _panel.SetActive(false);
        }

        private void OnEnable()
        {
            EventBus.OnPlayerDeath += HandlePlayerDeath;

            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestart);

            if (_menuButton != null)
                _menuButton.onClick.AddListener(OnMenu);
        }

        private void OnDisable()
        {
            EventBus.OnPlayerDeath -= HandlePlayerDeath;

            if (_restartButton != null)
                _restartButton.onClick.RemoveListener(OnRestart);

            if (_menuButton != null)
                _menuButton.onClick.RemoveListener(OnMenu);
        }

        private void HandlePlayerDeath()
        {
            if (_panel != null)
                _panel.SetActive(true);

            if (_levelReachedText != null && GameManager.Instance != null)
                _levelReachedText.text = $"You reached Level {GameManager.Instance.CurrentLevelIndex + 1}";

            // Pause game
            Time.timeScale = 0f;

            // Unlock cursor for button clicks
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnRestart()
        {
            if (_panel != null)
                _panel.SetActive(false);

            Time.timeScale = 1f;
            SceneController.LoadMain();
        }

        private void OnMenu()
        {
            if (_panel != null)
                _panel.SetActive(false);

            Time.timeScale = 1f;
            SceneController.LoadMenu();
        }
    }
}
