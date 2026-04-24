using UnityEngine;
using PrismPanic.Core;

public class DangerDetector : MonoBehaviour
{
    private bool _isAdrenalineActive = false;

    private void Update()
    {
        bool hasThreat = false;
        
        // We use OverlapSphere instead of triggers to guarantee it detects the angel
        // even if rigidbodies are missing or the sphere collider radius was forgotten.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, Constants.ADRENALINE_TRIGGER_RADIUS, 1 << Constants.LayerEnemy);
        
        foreach (var col in hitColliders)
        {
            // As long as there is an enemy in radius, we have a threat
            hasThreat = true;
            break;
        }

        if (hasThreat != _isAdrenalineActive)
        {
            _isAdrenalineActive = hasThreat;
            EventBus.FireAdrenalineStateChanged(_isAdrenalineActive);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draws a red sphere in the Scene view so you can visually see the danger zone!
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, Constants.ADRENALINE_TRIGGER_RADIUS);
    }
}
