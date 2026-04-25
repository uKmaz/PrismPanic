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
        [SerializeField] private float _baseWideAngle = Constants.BASE_WIDE_ANGLE;
        [SerializeField] private float _baseLaserAngle = Constants.BASE_LASER_ANGLE;
        [SerializeField] private float _baseBeamRange = Constants.BASE_BEAM_RANGE;
        [SerializeField] private float _baseMaxEnergy = Constants.BASE_MAX_ENERGY;
        [SerializeField] private int _basePlaceableMirrors = 0;

        // --- Runtime values (modified by upgrades, reset each run) ---
        [System.NonSerialized] public float moveSpeed;
        [System.NonSerialized] public float stunDuration;
        [System.NonSerialized] public float wideAngle;
        [System.NonSerialized] public float laserAngle;
        [System.NonSerialized] public float beamRange;
        [System.NonSerialized] public float maxEnergy;
        [System.NonSerialized] public float currentEnergy;
        [System.NonSerialized] public bool isOverheated;
        [System.NonSerialized] public int placeableMirrorCount; // Current room budget — resets each room
        [System.NonSerialized] public int mirrorsPerRoom;        // Permanent upgrade count — accumulates
        
        [System.NonSerialized] public int maxHP;
        [System.NonSerialized] public int currentHP;

        [System.NonSerialized] public int multishotCount;
        [System.NonSerialized] public float beamGrowthSpeed;
        [System.NonSerialized] public float energyDrainMultiplier;

        private void OnEnable()
        {
            ResetToDefaults();
        }

        public void ResetToDefaults()
        {
            moveSpeed = _baseMoveSpeed;
            stunDuration = _baseStunDuration;
            wideAngle = _baseWideAngle;
            laserAngle = _baseLaserAngle;
            beamRange = _baseBeamRange;
            maxEnergy = _baseMaxEnergy;
            currentEnergy = maxEnergy;
            isOverheated = false;
            mirrorsPerRoom = _basePlaceableMirrors;
            placeableMirrorCount = mirrorsPerRoom;

            // Player starts with 3 HP
            maxHP = 3;
            currentHP = maxHP;

            multishotCount = 1;
            beamGrowthSpeed = Constants.BEAM_GROWTH_SPEED;
            energyDrainMultiplier = 1.0f;
        }
    }
}
