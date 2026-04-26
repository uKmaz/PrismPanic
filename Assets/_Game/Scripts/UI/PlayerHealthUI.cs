using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PrismPanic.Core;
using PrismPanic.ScriptableObjects;

namespace PrismPanic.UI
{
    /// <summary>
    /// Minecraft-style segmented health bar with sprite-based breaking animation.
    /// Each HP is a separate heart that shows: Full → Cracking → Breaking → Empty
    /// when damage is taken. Dynamically adds segments when maxHP increases.
    /// </summary>
    public class PlayerHealthUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerStatsSO _playerStats;
        [SerializeField] private Transform _heartsContainer; // Horizontal Layout Group parent
        [SerializeField] private GameObject _heartPrefab;    // Prefab with an Image component

        [Header("Heart Sprites (in order)")]
        [Tooltip("Full healthy heart")]
        [SerializeField] private Sprite _heartFull;          // Normal red heart
        [Tooltip("Heart starting to crack")]
        [SerializeField] private Sprite _heartCracking;      // First break stage
        [Tooltip("Heart breaking apart")]
        [SerializeField] private Sprite _heartBreaking;      // Second break stage
        [Tooltip("Empty/broken heart")]
        [SerializeField] private Sprite _heartEmpty;         // Fully empty

        [Header("Animation")]
        [SerializeField] private float _breakFrameDuration = Constants.HEART_BREAK_FRAME_DURATION;

        private readonly List<Image> _heartImages = new List<Image>();
        private int _lastHP = -1;
        private int _lastMaxHP = -1;

        private void Start()
        {
            if (_playerStats != null)
            {
                RebuildHearts(_playerStats.maxHP);
                UpdateVisuals(_playerStats.currentHP);
            }
        }

        private void Update()
        {
            if (_playerStats == null) return;

            // Rebuild hearts if maxHP changed (upgrade)
            if (_playerStats.maxHP != _lastMaxHP)
            {
                RebuildHearts(_playerStats.maxHP);
            }

            // Update if HP changed
            if (_playerStats.currentHP != _lastHP)
            {
                int oldHP = _lastHP;
                int newHP = _playerStats.currentHP;

                // Damage taken — play breaking animation on lost hearts
                if (newHP < oldHP && oldHP > 0)
                {
                    for (int i = newHP; i < oldHP && i < _heartImages.Count; i++)
                    {
                        StartCoroutine(PlayBreakAnimation(i));
                    }
                }
                else
                {
                    // Healing or initial set — instant update
                    UpdateVisuals(newHP);
                }

                _lastHP = newHP;
            }
        }

        /// <summary>
        /// Spawns or removes heart segment GameObjects to match maxHP.
        /// </summary>
        private void RebuildHearts(int maxHP)
        {
            _lastMaxHP = maxHP;

            while (_heartImages.Count < maxHP)
            {
                if (_heartPrefab != null && _heartsContainer != null)
                {
                    GameObject heart = Instantiate(_heartPrefab, _heartsContainer);
                    Image img = heart.GetComponent<Image>();
                    if (img != null)
                    {
                        _heartImages.Add(img);
                    }
                }
            }

            while (_heartImages.Count > maxHP)
            {
                int lastIdx = _heartImages.Count - 1;
                Image toRemove = _heartImages[lastIdx];
                _heartImages.RemoveAt(lastIdx);
                if (toRemove != null)
                    Destroy(toRemove.gameObject);
            }

            UpdateVisuals(_playerStats != null ? _playerStats.currentHP : maxHP);
        }

        /// <summary>
        /// Instantly sets each heart to full or empty sprite. No position changes.
        /// </summary>
        private void UpdateVisuals(int currentHP)
        {
            _lastHP = currentHP;

            for (int i = 0; i < _heartImages.Count; i++)
            {
                if (_heartImages[i] == null) continue;
                _heartImages[i].sprite = i < currentHP ? _heartFull : _heartEmpty;
            }
        }

        /// <summary>
        /// Plays the breaking animation: Full → Cracking → Breaking → Empty.
        /// Only swaps sprites — no position changes.
        /// </summary>
        private IEnumerator PlayBreakAnimation(int heartIndex)
        {
            if (heartIndex < 0 || heartIndex >= _heartImages.Count) yield break;

            Image heart = _heartImages[heartIndex];
            if (heart == null) yield break;

            // Frame 1: Cracking
            heart.sprite = _heartCracking;
            yield return new WaitForSeconds(_breakFrameDuration);

            // Frame 2: Breaking
            heart.sprite = _heartBreaking;
            yield return new WaitForSeconds(_breakFrameDuration);

            // Frame 3: Empty
            heart.sprite = _heartEmpty;
        }
    }
}
