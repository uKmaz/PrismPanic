using UnityEngine;
using PrismPanic.Core;
using PrismPanic.Enemies;
using PrismPanic.ScriptableObjects;

namespace PrismPanic.Light
{
    /// <summary>
    /// Listens to OnBeamHit and applies stun or damage based on bounce count.
    /// bounceCount 0 = stun only, 1 = 1 damage, 2 = 2 damage (instant kill).
    /// </summary>
    public class BeamHitHandler : MonoBehaviour
    {
        [SerializeField] private PlayerStatsSO _playerStats;

        private void OnEnable()
        {
            EventBus.OnBeamHit += HandleBeamHit;
        }

        private void OnDisable()
        {
            EventBus.OnBeamHit -= HandleBeamHit;
        }

        private void HandleBeamHit(BeamHitData data)
        {
            if (data.target == null) return;

            // Check for Angel
            AngelController angel = data.target.GetComponent<AngelController>();
            if (angel != null)
            {
                HandleAngelHit(angel, data);
                return;
            }

            // Check for Boss — only reflected beams (bounceCount >= 1) damage the boss
            BossController boss = data.target.GetComponent<BossController>();
            if (boss != null && data.bounceCount >= 1)
            {
                boss.TakeDamage(data.bounceCount * 15);
                return;
            }
        }

        private void HandleAngelHit(AngelController angel, BeamHitData data)
        {

            switch (data.bounceCount)
            {
                case 0:
                    // Direct beam — stun only, no damage
                    float duration = _playerStats != null
                        ? _playerStats.stunDuration
                        : Constants.BASE_STUN_DURATION;
                    angel.Stun(duration);
                    break;

                case 1:
                    // 1-bounce — base damage
                    angel.TakeDamage(10);
                    break;

                case 2:
                    // 2-bounce — 2.5x damage
                    angel.TakeDamage(25);
                    break;

                case 3:
                    // 3-bounce — 5x damage or Tutorial Kill
                    if (GameManager.Instance != null && GameManager.Instance.CurrentLevelIndex == 0)
                    {
                        angel.TakeDamage(9999);
                    }
                    else
                    {
                        angel.TakeDamage(50);
                    }
                    break;
                    
                default:
                    // 4+ bounces
                    if (GameManager.Instance != null && GameManager.Instance.CurrentLevelIndex == 0)
                    {
                        // Tutorial Start condition — instant kill the tutorial angel
                        angel.TakeDamage(9999);
                    }
                    else
                    {
                        angel.TakeDamage(data.bounceCount * 15);
                    }
                    break;
            }
        }
    }
}
