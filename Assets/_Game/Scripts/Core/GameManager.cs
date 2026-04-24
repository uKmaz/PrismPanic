using UnityEngine;
using PrismPanic.ScriptableObjects;

namespace PrismPanic.Core
{
    /// <summary>
    /// Central game state machine. Owns phase, level index, active angel count.
    /// All transitions driven by EventBus — no direct method calls from other systems.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private PlayerStatsSO _playerStats;
        [SerializeField] private LevelManager _levelManager;

        public GamePhase CurrentPhase { get; private set; } = GamePhase.Boot;
        public int CurrentLevelIndex { get; private set; } = 0;
        public int ActiveAngelCount { get; private set; } = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.OnAngelKilled += HandleAngelKilled;
            EventBus.OnUpgradeSelected += HandleUpgradeSelected;
            EventBus.OnPlayerDeath += HandlePlayerDeath;
            EventBus.OnRoomReconfigureComplete += HandleRoomReady;
            EventBus.OnGameRestart += HandleRestart;
        }

        private void OnDisable()
        {
            EventBus.OnAngelKilled -= HandleAngelKilled;
            EventBus.OnUpgradeSelected -= HandleUpgradeSelected;
            EventBus.OnPlayerDeath -= HandlePlayerDeath;
            EventBus.OnRoomReconfigureComplete -= HandleRoomReady;
            EventBus.OnGameRestart -= HandleRestart;
        }

        private void Start()
        {
            StartNewRun();
        }

        public void StartNewRun()
        {
            CurrentLevelIndex = 0;
            _playerStats.ResetToDefaults();
            SetPhase(GamePhase.RoomSetup);

            LevelLayoutSO layout = _levelManager.GetLayout(CurrentLevelIndex);
            if (layout != null)
            {
                EventBus.FireRoomReconfigure(layout);
            }
        }

        /// <summary>
        /// Called by RoomConfigurator when spawning enemies to register count.
        /// </summary>
        public void SetAngelCount(int count)
        {
            ActiveAngelCount = count;
        }

        private void HandleAngelKilled(GameObject angel)
        {
            ActiveAngelCount--;
            Debug.Log($"[GameManager] Angel killed! Remaining: {ActiveAngelCount}");

            if (ActiveAngelCount <= 0)
            {
                ActiveAngelCount = 0;
                Debug.Log("[GameManager] ALL ANGELS CLEARED! Firing DoorsOpen...");
                SetPhase(GamePhase.DoorsOpen);
                EventBus.FireAllAngelsCleared();
                EventBus.FireDoorsOpen();
            }
        }

        private void HandleUpgradeSelected(ScriptableObject upgrade)
        {
            CurrentLevelIndex++;
            EventBus.FireLevelComplete();

            LevelLayoutSO nextLayout = _levelManager.GetLayout(CurrentLevelIndex);
            if (nextLayout != null)
            {
                SetPhase(GamePhase.RoomSetup);
                EventBus.FireRoomReconfigure(nextLayout);
            }
            else
            {
                // All levels complete — you win! Loop back for endless mode.
                CurrentLevelIndex = 0;
                LevelLayoutSO loopLayout = _levelManager.GetLayout(CurrentLevelIndex);
                SetPhase(GamePhase.RoomSetup);
                EventBus.FireRoomReconfigure(loopLayout);
            }
        }

        private void HandleRoomReady()
        {
            SetPhase(GamePhase.Combat);
            EventBus.FireLevelStart();
        }

        private void HandlePlayerDeath()
        {
            SetPhase(GamePhase.GameOver);
        }

        private void HandleRestart()
        {
            SceneController.LoadMain();
        }

        private void SetPhase(GamePhase phase)
        {
            CurrentPhase = phase;
        }
    }

    public enum GamePhase
    {
        Boot,
        RoomSetup,
        Combat,
        DoorsOpen,
        GameOver
    }
}
