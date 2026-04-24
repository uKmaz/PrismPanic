using System.Collections.Generic;
using UnityEngine;
using PrismPanic.ScriptableObjects;

namespace PrismPanic.Upgrades
{
    /// <summary>
    /// Holds all available upgrade definitions. Provides random selection
    /// excluding already-used upgrades per run.
    /// </summary>
    public class UpgradeRegistry : MonoBehaviour
    {
        [Tooltip("All upgrade definition assets")]
        [SerializeField] private UpgradeDefinitionSO[] _allUpgrades;

        /// <summary>
        /// Returns 'count' random upgrades not in the excluded set.
        /// </summary>
        public List<UpgradeDefinitionSO> GetRandomUpgrades(int count, HashSet<string> excludedIDs)
        {
            // Build eligible list
            List<UpgradeDefinitionSO> eligible = new List<UpgradeDefinitionSO>();
            foreach (var upgrade in _allUpgrades)
            {
                if (upgrade != null && !excludedIDs.Contains(upgrade.upgradeID))
                {
                    eligible.Add(upgrade);
                }
            }

            // Shuffle (Fisher-Yates)
            for (int i = eligible.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (eligible[i], eligible[j]) = (eligible[j], eligible[i]);
            }

            // Take up to count
            int take = Mathf.Min(count, eligible.Count);
            return eligible.GetRange(0, take);
        }
    }
}
