using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float maxHP = 30f;
    public float currentHP;
    public int soulsOnDeath = 1;
    private WaveManager waveManager;

    void Start()
    {
        currentHP = maxHP;
        waveManager = FindObjectOfType<WaveManager>();
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        if (waveManager != null)
        {
            waveManager.OnEnemyDied();
            waveManager.NotifyEnemyDestroyed(gameObject);
        }
        Destroy(gameObject);
    }
}