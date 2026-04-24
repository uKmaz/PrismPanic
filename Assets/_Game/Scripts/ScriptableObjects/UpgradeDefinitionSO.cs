using UnityEngine;

namespace PrismPanic.ScriptableObjects
{
    /// <summary>
    /// Defines a single upgrade. Create one asset per upgrade type.
    /// Upgrades: StunDuration+, MoveSpeed+, ConeWidth+, ExtraMirror, BeamRange+
    /// </summary>
    [CreateAssetMenu(menuName = "PrismPanic/UpgradeDefinition")]
    public class UpgradeDefinitionSO : ScriptableObject
    {
        public string upgradeID;
        public string displayName;
        public Sprite icon;
        [TextArea] public string description;
        public UpgradeEffectType effectType;
        public float effectValue;
    }

    public enum UpgradeEffectType
    {
        StunDurationBonus,
        MovementSpeedBonus,
        FlashlightConeWidth,
        ExtraMirrorPlacement,
        BeamRangeBonus
    }
}
