using UnityEngine;
using PrismPanic.Core;

namespace PrismPanic.ScriptableObjects
{
    /// <summary>
    /// Defines stats for an enemy type. Create assets per difficulty variant.
    /// Example: BaseAngel (speed 1.5, hp 2), FastAngel (speed 2.0, hp 2)
    /// </summary>
    [CreateAssetMenu(menuName = "PrismPanic/EnemyData")]
    public class EnemyDataSO : ScriptableObject
    {
        [Tooltip("Movement speed in units/sec")]
        public float moveSpeed = Constants.ANGEL_BASE_SPEED;

        [Tooltip("Hit points. 1-bounce = 1 dmg, 2-bounce = 2 dmg")]
        public int maxHP = Constants.ANGEL_BASE_HP;

        [Tooltip("Multiplier applied to stun duration (1 = normal)")]
        public float stunDurationModifier = 1f;
    }
}
