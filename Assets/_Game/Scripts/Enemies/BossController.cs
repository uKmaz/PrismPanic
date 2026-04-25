using UnityEngine;
using PrismPanic.Core;
using PrismPanic.Audio;

namespace PrismPanic.Enemies
{
    /// <summary>
    /// Boss Controller skeleton for the Boss Fight map.
    /// Handles Boss HP, taking damage from mirrors, and shooting rotating lasers.
    /// To be implemented further by the user's friend.
    /// </summary>
    public class BossController : MonoBehaviour
    {
        [Header("Boss Stats")]
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _currentHealth;
        
        [Header("Laser Settings")]
        [SerializeField] private Transform _laserOrigin; // Where the laser fires from
        [SerializeField] private float _laserRotationSpeed = 45f; // Degrees per second
        [SerializeField] private int _laserDamage = 20;

        // Components to be implemented:
        // - LineRenderer for the visual laser
        // - Raycast for damage detection
        // - State machine (Idle, Shooting, Vulnerable)

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        private void Update()
        {
            HandleLaserRotation();
            HandleLaserFiring();
        }

        private void HandleLaserRotation()
        {
            // Example: Rotate the boss or its laser origin around the Y axis
            if (_laserOrigin != null)
            {
                _laserOrigin.Rotate(Vector3.up, _laserRotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.Rotate(Vector3.up, _laserRotationSpeed * Time.deltaTime);
            }
        }

        private void HandleLaserFiring()
        {
            // TODO: Implement Raycast forward from _laserOrigin to hit the Player.
            // If Physics.Raycast hits PlayerController, call player.TakeDamage(_laserDamage).
            // Update LineRenderer positions to match the raycast.
        }

        /// <summary>
        /// Called by BeamHitHandler when a reflected beam hits the boss.
        /// </summary>
        public void TakeDamage(int damage)
        {
            _currentHealth -= damage;
            Debug.Log($"[Boss] Took {damage} damage! HP left: {_currentHealth}");

            // Optional: Play hit sound
            // AudioEffectHandler.Instance?.PlayBossHit();

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("[Boss] Defeated!");
            // Fire an event if necessary, or let GameManager handle victory
            EventBus.FireVictory(); // Instantly trigger victory when boss dies
            gameObject.SetActive(false);
        }
    }
}
