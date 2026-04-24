using UnityEngine;
using UnityEngine.UI;
using PrismPanic.Core;

namespace PrismPanic.UI
{
    /// <summary>
    /// Main menu. Title + Start + Quit buttons.
    /// Lives in the Menu scene (build index 0).
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _quitButton;

        private void Awake()
        {
            if (_startButton != null)
                _startButton.onClick.AddListener(OnStart);

            if (_quitButton != null)
                _quitButton.onClick.AddListener(OnQuit);
        }

        private void OnStart()
        {
            SceneController.LoadMain();
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
