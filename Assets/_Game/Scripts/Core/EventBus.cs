using System;
using UnityEngine;

namespace PrismPanic.Core
{
    /// <summary>
    /// Static event bus for all cross-system communication.
    /// Never call methods across system boundaries directly — use these events.
    /// </summary>
    public static class EventBus
    {
        // --- Combat ---
        public static event Action<GameObject> OnAngelStunned;
        public static event Action<GameObject> OnAngelKilled;
        public static event Action OnAllAngelsCleared;

        // --- Beam ---
        public static event Action<BeamHitData> OnBeamHit;

        // --- Room ---
        public static event Action<ScriptableObject> OnRoomReconfigure;
        public static event Action OnRoomReconfigureComplete;

        // --- Doors ---
        public static event Action OnDoorsOpen;
        public static event Action<ScriptableObject> OnUpgradeSelected;

        // --- Game State ---
        public static event Action OnLevelStart;
        public static event Action OnLevelComplete;
        public static event Action OnPlayerDeath;
        public static event Action OnGameRestart;
        public static event Action OnVictory;

        // --- Polish / Juice ---
        public static event Action<bool> OnAdrenalineStateChanged;

        // --- Fire Helpers (null-safe) ---
        public static void FireAngelStunned(GameObject angel) => OnAngelStunned?.Invoke(angel);
        public static void FireAngelKilled(GameObject angel) => OnAngelKilled?.Invoke(angel);
        public static void FireAllAngelsCleared() => OnAllAngelsCleared?.Invoke();
        public static void FireBeamHit(BeamHitData data) => OnBeamHit?.Invoke(data);
        public static void FireRoomReconfigure(ScriptableObject layout) => OnRoomReconfigure?.Invoke(layout);
        public static void FireRoomReconfigureComplete() => OnRoomReconfigureComplete?.Invoke();
        public static void FireDoorsOpen() => OnDoorsOpen?.Invoke();
        public static void FireUpgradeSelected(ScriptableObject upgrade) => OnUpgradeSelected?.Invoke(upgrade);
        public static void FireLevelStart() => OnLevelStart?.Invoke();
        public static void FireLevelComplete() => OnLevelComplete?.Invoke();
        public static void FirePlayerDeath() => OnPlayerDeath?.Invoke();
        public static void FireGameRestart() => OnGameRestart?.Invoke();
        public static void FireAdrenalineStateChanged(bool isActive) => OnAdrenalineStateChanged?.Invoke(isActive);
        public static void FireVictory() => OnVictory?.Invoke();

        /// <summary>
        /// Clears all subscribers. Call on scene unload to prevent leaks.
        /// </summary>
        public static void ClearAll()
        {
            OnAngelStunned = null;
            OnAngelKilled = null;
            OnAllAngelsCleared = null;
            OnBeamHit = null;
            OnRoomReconfigure = null;
            OnRoomReconfigureComplete = null;
            OnDoorsOpen = null;
            OnUpgradeSelected = null;
            OnLevelStart = null;
            OnLevelComplete = null;
            OnPlayerDeath = null;
            OnGameRestart = null;
            OnAdrenalineStateChanged = null;
            OnVictory = null;
        }
    }

    /// <summary>
    /// Data payload for beam hit events.
    /// </summary>
    public struct BeamHitData
    {
        public GameObject target;
        public int bounceCount;
        public Vector3 hitPoint;

        public BeamHitData(GameObject target, int bounceCount, Vector3 hitPoint)
        {
            this.target = target;
            this.bounceCount = bounceCount;
            this.hitPoint = hitPoint;
        }
    }
}
