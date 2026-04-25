using UnityEngine;
using PrismPanic.Core;

namespace PrismPanic.Audio
{
    /// <summary>
    /// Handles all sound effects. Listens to EventBus for global events, 
    /// and provides public methods for specific interactions (Walking, Lasers).
    /// </summary>
    public class AudioEffectHandler : MonoBehaviour
    {
        public static AudioEffectHandler Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _loopingSource; // For Heartbeat/Walking/Heating

        [Header("Sound Effects")]
        [SerializeField] private AudioClip _screamClip;
        [SerializeField] private AudioClip _heartbeatClip;
        [SerializeField] private AudioClip _explosionClip;
        [SerializeField] private AudioClip _laserClip;
        [SerializeField] private AudioClip _heatingClip;
        [SerializeField] private AudioClip _flashlightClickClip;
        [SerializeField] private AudioClip _walkingClip;
        [SerializeField] private AudioClip _doorOpeningClip;

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
            EventBus.OnDoorsOpen += HandleDoorsOpen;
            EventBus.OnAdrenalineStateChanged += HandleAdrenaline;
        }

        private void OnDisable()
        {
            EventBus.OnAngelKilled -= HandleAngelKilled;
            EventBus.OnDoorsOpen -= HandleDoorsOpen;
            EventBus.OnAdrenalineStateChanged -= HandleAdrenaline;
        }

        private void Update()
        {
            // Automatically handle heating sound if any angel is illuminated by the flashlight
            bool isAnyAngelHeating = PrismPanic.Light.AngelIlluminationRegistry.Count > 0;
            PlayHeating(isAnyAngelHeating);
        }

        // --- EventBus Driven ---

        private void HandleAngelKilled(GameObject angel)
        {
            PlayOneShot(_explosionClip);
            // Optionally play scream on death:
            // PlayOneShot(_screamClip);
        }

        private void HandleDoorsOpen()
        {
            PlayOneShot(_doorOpeningClip);
        }

        private void HandleAdrenaline(bool isActive)
        {
            if (isActive)
            {
                PlayLooping(_heartbeatClip);
                PlayOneShot(_screamClip); // Scream when spotted!
            }
            else
            {
                StopLooping(_heartbeatClip);
            }
        }

        // --- Public Methods for Player/Flashlight Scripts ---

        public void PlayLaser(int level)
        {
            if (_laserClip == null) return;
            
            // Pitch goes up based on the frequency level (1, 2, 3, 4)
            _sfxSource.pitch = 1f + (level * 0.15f); 
            _sfxSource.PlayOneShot(_laserClip);
            
            // Reset pitch after firing
            Invoke(nameof(ResetPitch), 0.5f);
        }

        public void PlayHeating(bool isHeating)
        {
            if (isHeating) PlayLooping(_heatingClip);
            else StopLooping(_heatingClip);
        }

        public void PlayWalking(bool isWalking)
        {
            // Only play walking if not already playing heartbeat or heating
            if (isWalking && _loopingSource.clip != _heartbeatClip) 
                PlayLooping(_walkingClip);
            else 
                StopLooping(_walkingClip);
        }

        public void PlayFlashlightClick()
        {
            PlayOneShot(_flashlightClickClip);
        }

        // --- Internal Helpers ---

        private void PlayOneShot(AudioClip clip)
        {
            if (clip != null && _sfxSource != null)
            {
                _sfxSource.PlayOneShot(clip);
            }
        }

        private void PlayLooping(AudioClip clip)
        {
            if (clip == null || _loopingSource == null) return;
            if (_loopingSource.clip == clip && _loopingSource.isPlaying) return;

            _loopingSource.clip = clip;
            _loopingSource.loop = true;
            _loopingSource.Play();
        }

        private void StopLooping(AudioClip clip)
        {
            if (_loopingSource != null && _loopingSource.clip == clip)
            {
                _loopingSource.Stop();
                _loopingSource.clip = null;
            }
        }

        private void ResetPitch()
        {
            if (_sfxSource != null) _sfxSource.pitch = 1f;
        }
    }
}
