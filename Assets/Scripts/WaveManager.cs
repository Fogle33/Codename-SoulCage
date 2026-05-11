using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Спавн")]
    public GameObject[] enemyPrefabs;       // Все три типа врагов
    public GameObject bossPrefab;
    public Transform[] spawnPoints;
    public Transform bossSpawnPoint;
    public int enemiesPerWave = 5;
    public int wavesMax = 10;
    public float enemyWait = 1f;
    public float spawnCheckRadius = 0.6f;
    public float spawnOffsetRadius = 0.5f;
    private float timeBetweenWaves;

    [Header("Таймер босса")]
    public float bossTimer = 120f;
    private float currentTimer;
    private bool bossSpawned = false;
    public ArenaUI UI;

    [Header("Динамика арены")]
    public float fastClearThreshold = 0.7f;
    public int bonusSoulsOnFastClear = 5;
    public float waveStrengthMultiplier = 1.1f;
    private float waveDifficulty = 1f;

    private int currentWave = 0;
    private int enemiesAlive = 0;
    private float waveStartTime;
    private GameObject[] lastSpawned;

    public void NotifyEnemyDestroyed(GameObject enemy)
    {
        if (lastSpawned == null) return;
        for (int i = 0; i < lastSpawned.Length; i++)
            if (lastSpawned[i] == enemy) lastSpawned[i] = null;
    }

    void Start()
    {
        timeBetweenWaves = bossTimer / wavesMax;
        currentTimer = bossTimer;
        if (spawnPoints != null)
            lastSpawned = new GameObject[spawnPoints.Length];
        if (UI != null) UI.UpdateWaves(currentWave, wavesMax);
        StartCoroutine(SpawnWave());
    }

    void Update()
    {
        if (!bossSpawned)
        {
            currentTimer -= Time.deltaTime;
            UI.UpdateTimer(currentTimer);
            if (currentTimer <= 0) SpawnBoss();
        }
    }

    IEnumerator SpawnWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        currentWave++;
        UI.UpdateWaves(currentWave, wavesMax);
        waveStartTime = Time.time;

        if (currentWave >= wavesMax && !bossSpawned)
        {
            SpawnBoss();
            yield break;
        }

        int count = Mathf.RoundToInt(enemiesPerWave * waveDifficulty);
        for (int i = 0; i < count; i++)
        {
            int maxAttempts = 10;
            int attempts = 0;
            int index = Random.Range(0, spawnPoints.Length);
            bool occupied = false;

            while (attempts < maxAttempts)
            {
                occupied = false;
                if (lastSpawned != null && index < lastSpawned.Length)
                {
                    var prev = lastSpawned[index];
                    if (prev != null)
                    {
                        if (prev.activeInHierarchy)
                        {
                            float dist = Vector3.Distance(prev.transform.position, spawnPoints[index].position);
                            if (dist < spawnCheckRadius) occupied = true;
                        }
                        else lastSpawned[index] = null;
                    }
                }
                if (!occupied) break;
                index = Random.Range(0, spawnPoints.Length);
                attempts++;
            }

            Vector3 spawnPos = spawnPoints[index].position;
            if (occupied)
            {
                Vector3 offset = Random.insideUnitSphere * spawnOffsetRadius;
                offset.z = 0f;
                spawnPos += offset;
            }

            // Случайный тип врага из массива
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            var enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            if (lastSpawned != null && index < lastSpawned.Length)
                lastSpawned[index] = enemy;

            enemiesAlive++;
            yield return null;
        }
    }

    public void OnEnemyDied()
    {
        enemiesAlive--;
        if (enemiesAlive <= 0)
            StartCoroutine(OnWaveCleared());
    }

    IEnumerator OnWaveCleared()
    {
        float clearTime = Time.time - waveStartTime;
        float waveDuration = timeBetweenWaves + (enemiesPerWave / 2f);

        if (clearTime < waveDuration * fastClearThreshold)
        {
            waveDifficulty *= waveStrengthMultiplier;
            Debug.Log("Быстрая зачистка!");
        }

        if (!bossSpawned)
            yield return StartCoroutine(SpawnWave());
    }

    void SpawnBoss()
    {
        bossSpawned = true;
        currentTimer = 0f;
        if (UI != null) UI.UpdateTimer(0f);
        if (bossPrefab != null && bossSpawnPoint != null)
            Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
    }
}