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

            AngelController angel = data.target.GetComponent<AngelController>();
            if (angel == null) return;

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
                    // 1-bounce — base damage (1 hit)
                    angel.TakeDamage(1);
                    break;

                case 2:
                    // 2-bounce — 2x damage (instant kill on 2HP angel)
                    angel.TakeDamage(2);
                    break;

                case 3:
                    // 3-bounce — 3x damage (heavy hit)
                    angel.TakeDamage(3);
                    break;
            }
        }
    }
}
