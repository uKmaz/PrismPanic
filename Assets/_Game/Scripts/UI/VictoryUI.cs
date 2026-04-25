using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using PrismPanic.Core;

namespace PrismPanic.UI
{
    /// <summary>
    /// Victory screen shown after completing all levels.
    /// MUST be on an ALWAYS-ACTIVE GameObject. Panel child gets toggled.
    /// R = Restart, Escape = Menu
    /// </summary>
    public class VictoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _statsText;

        private bool _isShowing;

        private void Awake()
        {
            if (_panel != null)
                _panel.SetActive(false);

            _isShowing = false;
        }

        private void OnEnable()
        {
            EventBus.OnVictory += HandleVictory;
        }

        private void OnDisable()
        {
            EventBus.OnVictory -= HandleVictory;
        }

        private void Update()
        {
            if (!_isShowing) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.rKey.wasPressedThisFrame)
            {
                OnRestart();
            }
            else if (kb.escapeKey.wasPressedThisFrame)
            {
                OnMenu();
            }
        }

        private void HandleVictory()
        {
            _isShowing = true;

            if (_panel != null)
                _panel.SetActive(true);

            if (_titleText != null)
                _titleText.text = "YOU WIN!";

            if (_statsText != null && GameManager.Instance != null)
                _statsText.text = $"All {GameManager.Instance.CurrentLevelIndex} levels completed!\n\nR - Play Again\nEsc - Menu";

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnRestart()
        {
            _isShowing = false;
            if (_panel != null)
                _panel.SetActive(false);

            Time.timeScale = 1f;
            EventBus.FireGameRestart();
        }

        private void OnMenu()
        {
            _isShowing = false;
            if (_panel != null)
                _panel.SetActive(false);

            Time.timeScale = 1f;
            SceneController.LoadMenu();
        }
    }
}
