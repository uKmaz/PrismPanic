using UnityEngine;
using UnityEngine.UI;
using PrismPanic.Core;
using PrismPanic.ScriptableObjects;

namespace PrismPanic.UI
{
    /// <summary>
    /// Updates a UI Slider to display the player's current flashlight energy.
    /// Changes color if the flashlight is overheated.
    /// Dynamically resizes the bar when maxEnergy increases from upgrades.
    /// </summary>
    public class EnergyUI : MonoBehaviour
    {
        [SerializeField] private PlayerStatsSO _playerStats;
        [SerializeField] private Slider _energySlider;
        [SerializeField] private Image _fillImage; // Assign the slider's Fill object here
        
        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.cyan;
        [SerializeField] private Color _overheatColor = Color.red;

        private float _baseWidth;
        private RectTransform _sliderRect;

        private void Start()
        {
            if (_energySlider != null)
            {
                _sliderRect = _energySlider.GetComponent<RectTransform>();
                _baseWidth = _sliderRect.sizeDelta.x;
            }
        }

        private void Update()
        {
            if (_playerStats != null && _energySlider != null)
            {
                // Prevent division by zero just in case
                if (_playerStats.maxEnergy > 0)
                {
                    _energySlider.value = _playerStats.currentEnergy / _playerStats.maxEnergy;

                    // Dynamically grow the bar width when maxEnergy increases
                    if (_sliderRect != null && _baseWidth > 0)
                    {
                        float scale = _playerStats.maxEnergy / Constants.BASE_MAX_ENERGY;
                        float targetWidth = _baseWidth * scale;
                        if (Mathf.Abs(_sliderRect.sizeDelta.x - targetWidth) > 0.5f)
                        {
                            _sliderRect.sizeDelta = new Vector2(
                                Mathf.Lerp(_sliderRect.sizeDelta.x, targetWidth, Time.deltaTime * 5f),
                                _sliderRect.sizeDelta.y
                            );
                        }
                    }
                }

                if (_fillImage != null)
                {
                    _fillImage.color = _playerStats.isOverheated ? _overheatColor : _normalColor;
                }
            }
        }
    }
}
