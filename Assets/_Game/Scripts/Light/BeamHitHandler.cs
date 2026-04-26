using UnityEngine;
using PrismPanic.Core;
using PrismPanic.Enemies;
using PrismPanic.ScriptableObjects;

namespace PrismPanic.Light
{
    /// <summary>
    /// Listens to OnBeamHit and applies stun or damage based on bounce count.
    /// All damage values are defined in Constants.cs — no magic numbers here.
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
                boss.TakeDamage(data.bounceCount * Constants.BEAM_DMG_MULTIPLIER);
                return;
            }
        }

        private void HandleAngelHit(AngelController angel, BeamHitData data)
        {
            bool isTutorial = GameManager.Instance != null && GameManager.Instance.CurrentLevelIndex == 0;

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
                    angel.TakeDamage(Constants.BEAM_DMG_1_BOUNCE);
                    break;

                case 2:
                    angel.TakeDamage(Constants.BEAM_DMG_2_BOUNCE);
                    break;

                case 3:
                    angel.TakeDamage(isTutorial ? Constants.BEAM_DMG_TUTORIAL_KILL : Constants.BEAM_DMG_3_BOUNCE);
                    break;
                    
                default:
                    // 4+ bounces
                    angel.TakeDamage(isTutorial ? Constants.BEAM_DMG_TUTORIAL_KILL : data.bounceCount * Constants.BEAM_DMG_MULTIPLIER);
                    break;
            }
        }
    }
}

