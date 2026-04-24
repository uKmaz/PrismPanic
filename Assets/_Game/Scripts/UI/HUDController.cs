using UnityEngine;
using TMPro;
using PrismPanic.Core;

namespace PrismPanic.UI
{
    /// <summary>
    /// HUD showing angel count and current level.
    /// Uses Update polling instead of events to avoid event ordering bugs.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text _angelCountText;
        [SerializeField] private TMP_Text _levelText;

        private int _lastAngelCount = -1;
        private int _lastLevel = -1;

        private void Update()
        {
            if (GameManager.Instance == null) return;

            // Only update text when values actually change (avoid GC alloc every frame)
            int currentAngels = GameManager.Instance.ActiveAngelCount;
            if (currentAngels != _lastAngelCount)
            {
                _lastAngelCount = currentAngels;
                if (_angelCountText != null)
                    _angelCountText.text = $"Angels: {currentAngels}";
            }

            int currentLevel = GameManager.Instance.CurrentLevelIndex;
            if (currentLevel != _lastLevel)
            {
                _lastLevel = currentLevel;
                if (_levelText != null)
                    _levelText.text = $"Level {currentLevel + 1}";
            }
        }
    }
}
