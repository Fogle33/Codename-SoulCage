using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float maxHP = 30f;
    public float currentHP;
    public int soulsOnDeath = 1;
    public bool isBoss = false;

    private WaveManager waveManager;

    void Start()
    {
        Debug.Log("Enemy Start");
        currentHP = maxHP;
        waveManager = FindObjectOfType<WaveManager>();

        if (isBoss)
        {
            Debug.Log("Trying to show HP bar");
            if (BossHealthBar.Instance == null)
                Debug.LogError("BossHealthBar.Instance NULL");
            BossHealthBar.Instance?.Show(maxHP);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;

        if (isBoss)
            BossHealthBar.Instance?.UpdateHP(currentHP);

        if (currentHP <= 0) Die();
    }

    void Die()
    {
        if (isBoss)
            BossHealthBar.Instance?.Hide();

        if (waveManager != null)
        {
            waveManager.OnEnemyDied();
            waveManager.NotifyEnemyDestroyed(gameObject);
        }

        Destroy(gameObject);
    }
}