using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using PrismPanic.Core;

namespace PrismPanic.UI
{
    /// <summary>
    /// Victory screen shown after completing all levels.
    /// MUST be on an ALWAYS-ACTIVE GameObject. Panel child gets toggled.
    /// </summary>
    public class VictoryUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;

        private bool _isShowing;

        private void Awake()
        {
            HidePanel();
        }

        private void Start()
        {
            // Failsafe: Ensure it's hidden when the scene fully starts
            HidePanel();
        }

        private void OnEnable()
        {
            EventBus.OnVictory += HandleVictory;
            EventBus.OnRoomReconfigureComplete += HidePanel;
            
            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestart);
                
            if (_menuButton != null)
                _menuButton.onClick.AddListener(OnMenu);
        }

        private void OnDisable()
        {
            EventBus.OnVictory -= HandleVictory;
            EventBus.OnRoomReconfigureComplete -= HidePanel;
            
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

        private void HandleVictory()
        {
            _isShowing = true;

            if (_panel != null)
                _panel.SetActive(true);

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void OnRestart()
        {
            HidePanel();
            Time.timeScale = 1f;
            EventBus.FireGameRestart();
        }

        public void OnMenu()
        {
            HidePanel();
            Time.timeScale = 1f;
            SceneController.LoadMenu();
        }

        private void HidePanel()
        {
            _isShowing = false;
            if (_panel != null)
            {
                _panel.SetActive(false);
            }
            else if (transform.childCount > 0)
            {
                // Fallback if panel is not assigned in Inspector!
                transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }
}
