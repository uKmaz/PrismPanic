using UnityEngine;
using PrismPanic.Core;

namespace PrismPanic.ScriptableObjects
{
    /// <summary>
    /// Runtime player stats. NonSerialized fields reset each run via ResetToDefaults().
    /// Create ONE asset: Assets/_Game/ScriptableObjects/PlayerStats/DefaultPlayerStats.asset
    /// </summary>
    [CreateAssetMenu(menuName = "PrismPanic/PlayerStats")]
    public class PlayerStatsSO : ScriptableObject
    {
        [Header("Base Values (serialized — don't modify at runtime)")]
        [SerializeField] private float _baseMoveSpeed = Constants.BASE_MOVE_SPEED;
        [SerializeField] private float _baseStunDuration = Constants.BASE_STUN_DURATION;
        [SerializeField] private float _baseConeAngle = Constants.BASE_CONE_ANGLE;
        [SerializeField] private float _baseBeamRange = Constants.BASE_BEAM_RANGE;
        [SerializeField] private int _basePlaceableMirrors = 0;

        // --- Runtime values (modified by upgrades, reset each run) ---
        [System.NonSerialized] public float moveSpeed;
        [System.NonSerialized] public float stunDuration;
        [System.NonSerialized] public float flashlightConeAngle;
        [System.NonSerialized] public float beamRange;
        [System.NonSerialized] public int placeableMirrorCount;

        private void OnEnable()
        {
            ResetToDefaults();
        }

        public void ResetToDefaults()
        {
            moveSpeed = _baseMoveSpeed;
            stunDuration = _baseStunDuration;
            flashlightConeAngle = _baseConeAngle;
            beamRange = _baseBeamRange;
            placeableMirrorCount = _basePlaceableMirrors;
        }
    }
}
