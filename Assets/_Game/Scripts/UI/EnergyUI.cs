using UnityEngine;
using UnityEngine.UI;
using PrismPanic.ScriptableObjects;

namespace PrismPanic.UI
{
    /// <summary>
    /// Updates a UI Slider to display the player's current flashlight energy.
    /// Changes color if the flashlight is overheated.
    /// </summary>
    public class EnergyUI : MonoBehaviour
    {
        [SerializeField] private PlayerStatsSO _playerStats;
        [SerializeField] private Slider _energySlider;
        [SerializeField] private Image _fillImage; // Assign the slider's Fill object here
        
        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.cyan;
        [SerializeField] private Color _overheatColor = Color.red;

        private void Update()
        {
            if (_playerStats != null && _energySlider != null)
            {
                // Prevent division by zero just in case
                if (_playerStats.maxEnergy > 0)
                {
                    _energySlider.value = _playerStats.currentEnergy / _playerStats.maxEnergy;
                }

                if (_fillImage != null)
                {
                    _fillImage.color = _playerStats.isOverheated ? _overheatColor : _normalColor;
                }
            }
        }
    }
}
