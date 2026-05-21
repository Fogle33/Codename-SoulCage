using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnableEnemy
{
    public GameObject prefab;
    public int spawnWeight = 10;
    public float damage = 10f;
    public float attackCooldown = 1f;
    public float importance = 1f;

    public int GetCost(float playerDPS, float playerHP, float constantK)
    {
        var stats = prefab != null ? prefab.GetComponent<EnemyStats>() : null;
        if (stats == null) return 1;

        float ttkByPlayer = stats.maxHP / playerDPS;
        float enemyDPS = attackCooldown > 0 ? damage / attackCooldown : 1f;
        float ttkOfPlayer = enemyDPS > 0 ? playerHP / enemyDPS : 99f;
        return Mathf.Max(1, Mathf.RoundToInt((ttkByPlayer / ttkOfPlayer) * importance * constantK));
    }
}

public class WaveManager : MonoBehaviour
{
    [Header("Враги")]
    public SpawnableEnemy[] enemyTypes;
    public GameObject bossPrefab;

    [Header("Спавн точки")]
    public Transform[] spawnPoints;
    public Transform bossSpawnPoint;

    [Header("Параметры игрока (для расчёта стоимости)")]
    public float playerHP = 100f;
    public float playerDamage = 10f;
    public float playerAttackCooldown = 0.5f;
    public float constantK = 5f;

    [Header("Бюджеты волн")]
    public int[] waveBudgets = { 21, 23, 25, 28 };

    [Header("Таймер")]
    public float waveInterval = 15f;
    public float firstWaveDelay = 2f;

    [Header("Динамика")]
    public int budgetPenaltyPerAliveEnemy = 1;
    public float fastClearThreshold = 0.5f;
    public int bonusSoulsOnFastClear = 5;

    [Header("Переход после босса")]
    public string nextScene = "PurchaseScreen";
    public bool isFinalArena = false;

    public ArenaUI UI;

    private int currentWave = 0;
    private int enemiesAlive = 0;
    private float waveTimer;
    private float waveStartTime;
    private bool waveActive = false;
    private bool bossSpawned = false;
    private int pendingPenalty = 0;
    private float playerDPS;

    void Start()
    {
        playerDPS = playerDamage / playerAttackCooldown;
        if (UI != null) UI.UpdateWaves(0, waveBudgets.Length);
        waveTimer = firstWaveDelay;
    }

    void Update()
    {
        if (bossSpawned || waveActive) return;
        waveTimer -= Time.deltaTime;
        if (UI != null) UI.UpdateTimer(waveTimer);
        if (waveTimer <= 0f) StartNextWave();
    }

    void StartNextWave()
    {
        if (waveActive || bossSpawned) return;
        if (currentWave >= waveBudgets.Length) { SpawnBoss(); return; }
        StartCoroutine(RunWave());
    }

    IEnumerator RunWave()
    {
        waveActive = true;
        currentWave++;
        if (UI != null) UI.UpdateWaves(currentWave, waveBudgets.Length);
        waveStartTime = Time.time;

        int budget = Mathf.Max(waveBudgets[currentWave - 1] - pendingPenalty, 3);
        pendingPenalty = 0;

        while (budget > 0)
        {
            var chosen = PickEnemy(budget);
            if (chosen == null) break;
            int cost = chosen.GetCost(playerDPS, playerHP, constantK);
            int idx = Random.Range(0, spawnPoints.Length);
            Instantiate(chosen.prefab, spawnPoints[idx].position, Quaternion.identity);
            enemiesAlive++;
            budget -= cost;
            yield return new WaitForSeconds(0.3f);
        }

        waveActive = false;
        waveTimer = waveInterval;
        if (enemiesAlive <= 0) StartNextWave();
    }

    SpawnableEnemy PickEnemy(int budget)
    {
        var pool = new List<SpawnableEnemy>();
        foreach (var e in enemyTypes)
        {
            int cost = e.GetCost(playerDPS, playerHP, constantK);
            if (cost <= budget) pool.Add(e);
        }
        if (pool.Count == 0) return null;

        int total = 0;
        foreach (var e in pool) total += e.spawnWeight;
        int roll = Random.Range(0, total);
        int acc = 0;
        foreach (var e in pool)
        {
            acc += e.spawnWeight;
            if (roll < acc) return e;
        }
        return pool[pool.Count - 1];
    }

    public void OnEnemyDied()
    {
        enemiesAlive--;
        if (enemiesAlive <= 0 && !waveActive && !bossSpawned)
        {
            float t = Time.time - waveStartTime;
            if (t < waveInterval * fastClearThreshold)
                GameState.AddSouls(bonusSoulsOnFastClear);
            pendingPenalty = 0;
            StartNextWave();
        }
    }

    public void OnBossDefeated()
    {
        if (isFinalArena)
            StartCoroutine(FinalSequence());
        else
        {
            GameState.NextScene = nextScene;
            var t = FindObjectOfType<SceneTransition>();
            if (t != null) t.LoadScene("PurchaseScreen");
            else UnityEngine.SceneManagement.SceneManager.LoadScene("PurchaseScreen");
        }
    }

    IEnumerator FinalSequence()
    {
        yield return new WaitForSeconds(2f);
        var t = FindObjectOfType<SceneTransition>();
        if (t != null) t.LoadScene("Hub");
        else UnityEngine.SceneManagement.SceneManager.LoadScene("Hub");
    }

    public void NotifyEnemyDestroyed(GameObject enemy) { }

    void SpawnBoss()
    {
        if (bossSpawned) return;
        bossSpawned = true;
        if (UI != null) UI.UpdateTimer(0f);
        if (bossPrefab != null && bossSpawnPoint != null)
            Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
    }
}