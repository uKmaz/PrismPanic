using System.Collections;
using UnityEngine;
using PrismPanic.Core;

namespace PrismPanic.Audio
{
    /// <summary>
    /// Manages background music tracks and crossfading between them based on GamePhase.
    /// Boss rooms get action music. Room clears get a victory jingle. Restart resets everything.
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
        [SerializeField] private AudioClip _roomClearedMusic;

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
            DontDestroyOnLoad(gameObject);

            // Music gets highest priority (0) — never culled by Unity's voice limit
            if (_musicSource1 != null) _musicSource1.priority = 0;
            if (_musicSource2 != null) _musicSource2.priority = 0;
        }

        private void OnEnable()
        {
            EventBus.OnLevelStart += HandleCombatStart;
            EventBus.OnDoorsOpen += HandleDoorsOpen;
            EventBus.OnPlayerDeath += HandleGameOver;
            EventBus.OnVictory += HandleVictory;
            EventBus.OnGameRestart += HandleRestart;
            EventBus.OnRoomReconfigure += HandleRoomReconfigure;
        }

        private void OnDisable()
        {
            EventBus.OnLevelStart -= HandleCombatStart;
            EventBus.OnDoorsOpen -= HandleDoorsOpen;
            EventBus.OnPlayerDeath -= HandleGameOver;
            EventBus.OnVictory -= HandleVictory;
            EventBus.OnGameRestart -= HandleRestart;
            EventBus.OnRoomReconfigure -= HandleRoomReconfigure;
        }

        private void Start()
        {
            // Start with Thrilling or Main Menu music
            PlayMusic(_thrillingMusic);
        }

        private void HandleCombatStart()
        {
            // Boss rooms get action music, normal rooms get thrilling
            if (GameManager.Instance != null && GameManager.Instance.IsBossRoom)
            {
                PlayMusic(_actionMusic);
            }
            else
            {
                PlayMusic(_thrillingMusic);
            }
        }

        private void HandleDoorsOpen()
        {
            // Play room cleared jingle when all angels are dead
            PlayMusic(_roomClearedMusic);
        }

        private void HandleRoomReconfigure(ScriptableObject layout)
        {
            // New room loading — stop room cleared music, switch to thrilling
            PlayMusic(_thrillingMusic);
        }

        private void HandleGameOver() => PlayMusic(_endGameMusic);
        private void HandleVictory() => PlayMusic(_endGameMusic);

        private void HandleRestart()
        {
            // Full reset — stop everything, start fresh with thrilling
            StopAllCoroutines();
            if (_musicSource1 != null) { _musicSource1.Stop(); _musicSource1.volume = 0; }
            if (_musicSource2 != null) { _musicSource2.Stop(); _musicSource2.volume = 0; }
            _isUsingSource1 = true;
            PlayMusic(_thrillingMusic);
        }

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
            float maxVolume = 0.7f;

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

