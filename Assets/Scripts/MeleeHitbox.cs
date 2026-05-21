using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    public float damage = 10f;
    public float healPercent = 0.01f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyStats>()?.TakeDamage(damage);
            var stats = GetComponentInParent<PlayerStats>();
            if (stats != null)
                stats.Heal(stats.maxHP * PlayerUpgrades.HealMultiplier);
        }
    }
}