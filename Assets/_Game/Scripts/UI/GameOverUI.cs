using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using PrismPanic.Core;

namespace PrismPanic.UI
{
    /// <summary>
    /// Game Over screen. MUST be on an ALWAYS-ACTIVE GameObject.
    /// Keyboard shortcuts: R = Restart, Escape = Menu
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _levelReachedText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;

        private bool _isShowing;

        private void Awake()
        {
            if (_panel != null)
                _panel.SetActive(false);

            _isShowing = false;
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

        private void HandlePlayerDeath()
        {
            StartCoroutine(DeathSequenceRoutine());
        }

        private IEnumerator DeathSequenceRoutine()
        {
            _isShowing = true;
            
            // Start dramatic slow motion
            Time.timeScale = 0.3f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            
            // Camera Shake
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                Vector3 originalPos = mainCam.transform.localPosition;
                float shakeDuration = 1.5f; // Will be scaled by timescale, so feels longer
                
                while (shakeDuration > 0)
                {
                    mainCam.transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * 0.3f;
                    shakeDuration -= Time.unscaledDeltaTime;
                    yield return null;
                }
                mainCam.transform.localPosition = originalPos;
            }
            else
            {
                yield return new WaitForSecondsRealtime(1.5f);
            }

            // Stop time completely and show UI
            Time.timeScale = 0f;
            Time.fixedDeltaTime = 0.02f;

            if (_panel != null)
                _panel.SetActive(true);

            if (_levelReachedText != null && GameManager.Instance != null)
                _levelReachedText.text = $"You reached Level {GameManager.Instance.CurrentLevelIndex + 1}";

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnRestart()
        {
            _isShowing = false;
            if (_panel != null)
                _panel.SetActive(false);

            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            EventBus.FireGameRestart();
        }

        private void OnMenu()
        {
            _isShowing = false;
            if (_panel != null)
                _panel.SetActive(false);

            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            SceneController.LoadMenu();
        }
    }
}
