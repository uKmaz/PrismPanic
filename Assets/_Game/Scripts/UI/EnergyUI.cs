using UnityEngine;
using UnityEngine.UI;
using PrismPanic.Core;
using PrismPanic.ScriptableObjects;

namespace PrismPanic.UI
{
    /// <summary>
    /// Updates a UI Slider to display the player's current flashlight energy.
    /// Changes color if the flashlight is overheated.
    /// Fixed-size bar — no dynamic resizing. Resolution-independent via Canvas Scaler.
    /// </summary>
    public class EnergyUI : MonoBehaviour
    {
        [SerializeField] private PlayerStatsSO _playerStats;
        [SerializeField] private Slider _energySlider;
        [SerializeField] private Image _fillImage; // Assign the slider's Fill object here

        [Header("Overheat Threshold Marker")]
        [Tooltip("Optional: an Image placed at the 70% mark to show the overheat line")]
        [SerializeField] private RectTransform _overheatMarker;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.cyan;
        [SerializeField] private Color _overheatColor = Color.red;

        private void Start()
        {
            PositionOverheatMarker();
        }

        private void Update()
        {
            if (_playerStats == null || _energySlider == null) return;

            if (_playerStats.maxEnergy > 0)
            {
                _energySlider.value = _playerStats.currentEnergy / _playerStats.maxEnergy;
            }

            if (_fillImage != null)
            {
                _fillImage.color = _playerStats.isOverheated ? _overheatColor : _normalColor;
            }
        }

        /// <summary>
        /// Places the overheat marker at exactly 70% of the slider width.
        /// Uses anchors so it stays correct at any resolution.
        /// </summary>
        private void PositionOverheatMarker()
        {
            if (_overheatMarker == null || _energySlider == null) return;

            // Parent the marker under the slider so it scales with it
            _overheatMarker.SetParent(_energySlider.transform, false);

            // Use anchors to pin at 70% horizontally, stretch vertically
            _overheatMarker.anchorMin = new Vector2(Constants.ENERGY_OVERHEAT_THRESHOLD, 0f);
            _overheatMarker.anchorMax = new Vector2(Constants.ENERGY_OVERHEAT_THRESHOLD, 1f);
            _overheatMarker.pivot = new Vector2(0.5f, 0.5f);

            // Thin vertical line: 3px wide, full height
            _overheatMarker.sizeDelta = new Vector2(3f, 0f);
            _overheatMarker.anchoredPosition = Vector2.zero;
        }
    }
}
