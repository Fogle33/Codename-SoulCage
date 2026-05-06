using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float maxHP = 30f;
    public float currentHP;
    public int soulsOnDeath = 1; // Сколько душ даёт при смерти

    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        // Позже здесь будет: SoulManager.Instance.AddSouls(soulsOnDeath)
        // Уведомляем WaveManager о смерти, чтобы он уменьшил счётчик и очистил ссылку lastSpawned
        WaveManager wm = FindObjectOfType<WaveManager>();
        if (wm != null)
        {
            wm.OnEnemyDied();
            wm.NotifyEnemyDestroyed(gameObject);
        }

        Destroy(gameObject);
    }
}
