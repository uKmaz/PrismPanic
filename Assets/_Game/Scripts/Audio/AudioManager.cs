using System.Collections;
using UnityEngine;
using PrismPanic.Core;

namespace PrismPanic.Audio
{
    /// <summary>
    /// Manages background music tracks and crossfading between them based on GamePhase.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource1;
        [SerializeField] private AudioSource _musicSource2;

        [Header("Music Tracks")]
        [SerializeField] private AudioClip _mainScreenMusic;
        [SerializeField] private AudioClip _endGameMusic;
        [SerializeField] private AudioClip _actionMusic;
        [SerializeField] private AudioClip _thrillingMusic;

        [Header("Settings")]
        [SerializeField] private float _crossfadeDuration = 2.0f;

        private bool _isUsingSource1 = true;

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
            // We can listen to GamePhase changes to automatically swap music!
            EventBus.OnLevelStart += HandleCombatStart;
            EventBus.OnDoorsOpen += HandleDoorsOpen;
            EventBus.OnPlayerDeath += HandleGameOver;
            EventBus.OnVictory += HandleVictory;
            EventBus.OnGameRestart += HandleRestart;
        }

        private void OnDisable()
        {
            EventBus.OnLevelStart -= HandleCombatStart;
            EventBus.OnDoorsOpen -= HandleDoorsOpen;
            EventBus.OnPlayerDeath -= HandleGameOver;
            EventBus.OnVictory -= HandleVictory;
            EventBus.OnGameRestart -= HandleRestart;
        }

        private void Start()
        {
            // Start with Thrilling or Main Menu music
            PlayMusic(_thrillingMusic);
        }

        private void HandleCombatStart() => PlayMusic(_actionMusic);
        private void HandleDoorsOpen() => PlayMusic(_thrillingMusic);
        private void HandleGameOver() => PlayMusic(_endGameMusic);
        private void HandleVictory() => PlayMusic(_endGameMusic);
        private void HandleRestart() => PlayMusic(_thrillingMusic);

        public void PlayMusic(AudioClip newClip)
        {
            if (newClip == null) return;

            AudioSource activeSource = _isUsingSource1 ? _musicSource1 : _musicSource2;
            if (activeSource.clip == newClip && activeSource.isPlaying) return;

            _isUsingSource1 = !_isUsingSource1;
            AudioSource newSource = _isUsingSource1 ? _musicSource1 : _musicSource2;
            AudioSource fadingSource = _isUsingSource1 ? _musicSource2 : _musicSource1;

            newSource.clip = newClip;
            newSource.Play();

            StopAllCoroutines();
            StartCoroutine(Crossfade(fadingSource, newSource, _crossfadeDuration));
        }

        private IEnumerator Crossfade(AudioSource fadeOut, AudioSource fadeIn, float duration)
        {
            float time = 0;
            fadeIn.volume = 0;
            float maxVolume = 1f; // Modify this if you have a master volume setting

            while (time < duration)
            {
                time += Time.deltaTime;
                fadeIn.volume = Mathf.Lerp(0, maxVolume, time / duration);
                fadeOut.volume = Mathf.Lerp(maxVolume, 0, time / duration);
                yield return null;
            }

            fadeIn.volume = maxVolume;
            fadeOut.volume = 0;
            fadeOut.Stop();
        }
    }
}
