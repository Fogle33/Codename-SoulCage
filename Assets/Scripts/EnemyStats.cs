using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float maxHP = 30f;
    public float currentHP;
    public int soulsOnDeath = 1;
    public bool isBoss = false;

    [Header("Scrap drop")]
    [Range(0, 100)] public int scrapDropChance = 10;
    public int scrapAmount = 1;

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
        GameState.AddSouls(soulsOnDeath);
        if (Random.Range(0, 100) < scrapDropChance)
            GameState.AddScrap(scrapAmount);

        if (isBoss)
        {
            BossHealthBar.Instance?.Hide();
            waveManager?.OnBossDefeated();
        }
        else
        {
            waveManager?.OnEnemyDied();
            waveManager?.NotifyEnemyDestroyed(gameObject);
        }

        Destroy(gameObject);
    }
}