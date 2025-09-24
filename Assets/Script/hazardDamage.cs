using System.Threading;
using UnityEngine;

public class hazardDamage : MonoBehaviour
{
    public int damageAmount = 1;
    public enum HazardType { once, untilExitCollider }
    public HazardType hazardType = HazardType.once;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hazardType == HazardType.once)
        {
            ApplyDamage(collision);
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (hazardType == HazardType.untilExitCollider)
        {
            ApplyDamage(collision);
        }
    }

    public void ApplyDamage(Collider2D collision)
    {
        UnityEngine.Debug.Log("Applying damage to " + collision.name);
        Health health = collision.GetComponent<Health>();
        if (health != null && health.isInvincibleStatus() == false)
        {
            UnityEngine.Debug.Log("Damaging " + collision.name);
            health.TakeDamage(damageAmount);
        }
    }
}
