using UnityEngine;
using PrismPanic.Core;

namespace PrismPanic.Audio
{
    /// <summary>
    /// Handles all sound effects with proper contextual logic:
    /// - Laser: loops while flashlight is in Laser mode
    /// - Walking: loops while player is moving
    /// - Heartbeat: loops while an angel is nearby (adrenaline)
    /// - Heating: loops while beam is hitting an angel
    /// - Door: plays when player presses F to enter a door
    /// - Explosion: plays on angel death
    /// - Flashlight click: plays on mode change
    /// 
    /// Uses 3 AudioSources: one-shot SFX, a looping ambient source, and a laser source.
    /// Priority system prevents lower-priority loops from overriding higher ones.
    /// </summary>
    public class AudioEffectHandler : MonoBehaviour
    {
        public static AudioEffectHandler Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _sfxSource;
        [Tooltip("For ambient loops: heartbeat, walking")]
        [SerializeField] private AudioSource _loopingSource;
        [Tooltip("Dedicated source for laser hum — won't fight with other loops")]
        [SerializeField] private AudioSource _laserLoopSource;

        [Header("One-Shot Sound Effects")]
        [SerializeField] private AudioClip _screamClip;
        [SerializeField] private AudioClip _explosionClip;
        [SerializeField] private AudioClip _flashlightClickClip;
        [SerializeField] private AudioClip _doorOpeningClip;

        [Header("Looping Sound Effects")]
        [SerializeField] private AudioClip _heartbeatClip;
        [SerializeField] private AudioClip _laserClip;
        [SerializeField] private AudioClip _heatingClip;
        [SerializeField] private AudioClip _walkingClip;

        // State tracking
        private bool _isAdrenalineActive;
        private bool _isLaserActive;
        private bool _isHeating;
        private bool _isWalking;

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
            EventBus.OnAdrenalineStateChanged += HandleAdrenaline;
            EventBus.OnUpgradeSelected += HandleUpgradeSelected;
        }

        private void OnDisable()
        {
            EventBus.OnAngelKilled -= HandleAngelKilled;
            EventBus.OnAdrenalineStateChanged -= HandleAdrenaline;
            EventBus.OnUpgradeSelected -= HandleUpgradeSelected;
        }

        private void Update()
        {
            // --- Heating: beam is hitting an angel ---
            bool isAnyAngelHeating = PrismPanic.Light.AngelIlluminationRegistry.Count > 0;
            if (isAnyAngelHeating != _isHeating)
            {
                _isHeating = isAnyAngelHeating;
            }

            // --- Update ambient loop priority ---
            UpdateAmbientLoop();
        }

        // ====================================================
        //  EVENT HANDLERS
        // ====================================================

        private void HandleAngelKilled(GameObject angel)
        {
            PlayOneShot(_explosionClip);
        }

        private void HandleAdrenaline(bool isActive)
        {
            _isAdrenalineActive = isActive;
            if (isActive)
            {
                PlayOneShot(_screamClip);
            }
        }

        /// <summary>
        /// Door sound when player picks an upgrade (presses F).
        /// </summary>
        private void HandleUpgradeSelected(ScriptableObject upgrade)
        {
            PlayOneShot(_doorOpeningClip);
        }

        // ====================================================
        //  PUBLIC METHODS — Called by Player/Flashlight scripts
        // ====================================================

        /// <summary>
        /// Call from FlashlightController when mode changes.
        /// Manages laser loop start/stop and plays click sound.
        /// </summary>
        public void SetFlashlightMode(FlashlightMode mode)
        {
            bool wasLaser = _isLaserActive;
            _isLaserActive = (mode == FlashlightMode.Laser);

            // Laser loop
            if (_isLaserActive && !wasLaser)
            {
                StartLaserLoop();
            }
            else if (!_isLaserActive && wasLaser)
            {
                StopLaserLoop();
            }

            // Click sound on any mode change
            PlayOneShot(_flashlightClickClip);
        }

        /// <summary>
        /// Call from PlayerController each frame with current movement state.
        /// </summary>
        public void SetWalking(bool isWalking)
        {
            _isWalking = isWalking;
        }

        // ====================================================
        //  AMBIENT LOOP PRIORITY SYSTEM
        //  Priority: Heartbeat > Heating > Walking > Silence
        //  Laser has its own dedicated source so it never fights.
        // ====================================================

        private void UpdateAmbientLoop()
        {
            AudioClip desired = null;

            // Priority order — highest first
            if (_isAdrenalineActive && _heartbeatClip != null)
                desired = _heartbeatClip;
            else if (_isHeating && _heatingClip != null)
                desired = _heatingClip;
            else if (_isWalking && _walkingClip != null)
                desired = _walkingClip;

            if (_loopingSource == null) return;

            if (desired != null)
            {
                if (_loopingSource.clip != desired || !_loopingSource.isPlaying)
                {
                    _loopingSource.clip = desired;
                    _loopingSource.loop = true;
                    _loopingSource.Play();
                }
            }
            else
            {
                if (_loopingSource.isPlaying)
                {
                    _loopingSource.Stop();
                    _loopingSource.clip = null;
                }
            }
        }

        // ====================================================
        //  LASER LOOP (dedicated source)
        // ====================================================

        private void StartLaserLoop()
        {
            if (_laserLoopSource == null || _laserClip == null) return;
            if (_laserLoopSource.isPlaying && _laserLoopSource.clip == _laserClip) return;

            _laserLoopSource.clip = _laserClip;
            _laserLoopSource.loop = true;
            _laserLoopSource.Play();
        }

        private void StopLaserLoop()
        {
            if (_laserLoopSource == null) return;
            _laserLoopSource.Stop();
            _laserLoopSource.clip = null;
        }

        // ====================================================
        //  HELPERS
        // ====================================================

        private void PlayOneShot(AudioClip clip)
        {
            if (clip != null && _sfxSource != null)
            {
                _sfxSource.PlayOneShot(clip);
            }
        }
    }
}
