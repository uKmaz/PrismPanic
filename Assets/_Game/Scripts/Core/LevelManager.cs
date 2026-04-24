using UnityEngine;
using PrismPanic.ScriptableObjects;

namespace PrismPanic.Core
{
    /// <summary>
    /// Holds the ordered array of LevelLayoutSO assets.
    /// Returns layouts by index. No runtime state beyond the array.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [Tooltip("Ordered list of level layouts. Level 0 = index 0, etc.")]
        [SerializeField] private LevelLayoutSO[] _layouts;

        /// <summary>
        /// Returns layout at given index, or null if out of range.
        /// </summary>
        public LevelLayoutSO GetLayout(int index)
        {
            if (_layouts == null || index < 0 || index >= _layouts.Length)
                return null;

            return _layouts[index];
        }

        public int TotalLevels => _layouts != null ? _layouts.Length : 0;
    }
}
