using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Спавн")]
    public GameObject enemyPrefab;          // Prefab врага
    public GameObject bossPrefab;           // Prefab босса
    public Transform[] spawnPoints;          // Точки спавна на арене
    public Transform bossSpawnPoint;       // Точка спавна босса
    public int enemiesPerWave = 5;              // Врагов в волне
    public int wavesMax = 10;                   // Всего волн
    public float enemyWait = 1f; //Время задержки между спавнами
    public float spawnCheckRadius = 0.6f; // Радиус, по которому считаем, что предыдущий юнит всё ещё занимает точку
    public float spawnOffsetRadius = 0.5f; // Максимальный небольшой оффсет, если все точки заняты
    public float timeBetweenWaves = 3f;      // Пауза между волнами

    [Header("Таймер босса")]
    public float bossTimer = 120f;           // Секунд до появления босса
    private float currentTimer;
    private bool bossSpawned = false;
    public ArenaUI UI;
    [Header("Динамика арены")]
    public float fastClearThreshold = 0.7f;  // 70% времени волны = быстрая зачистка
    public int bonusSoulsOnFastClear = 5;
    public float waveStrengthMultiplier = 1.1f; // Усиление следующей волны
    private float waveDifficulty = 1f;       // Текущий множитель сложности

    private int currentWave = 0;
    private int enemiesAlive = 0;
    private float waveStartTime;
    private GameObject[] lastSpawned; // Храним последний объект, заспавненный в каждой точке

    // Очищаем ссылку на последний заспавненный объект в точке, когда он умирает
    public void NotifyEnemyDestroyed(GameObject enemy)
    {
        if (lastSpawned == null) return;
        for (int i = 0; i < lastSpawned.Length; i++)
        {
            if (lastSpawned[i] == enemy)
                lastSpawned[i] = null;
        }
    }

    void Start()
    {
        currentTimer = bossTimer;
        // Инициализируем массив последних заспавненных объектов по точкам
        if (spawnPoints != null)
            lastSpawned = new GameObject[spawnPoints.Length];

        // Обновляем UI по волнам в начале
        if (UI != null) UI.UpdateWaves(currentWave, wavesMax);

        StartCoroutine(SpawnWave());
    }

    void Update()
    {
        // Отсчёт таймера босса
        if (!bossSpawned)
        {
            currentTimer -= Time.deltaTime;
            UI.UpdateTimer(currentTimer);
            // Позже здесь будет: UIManager.Instance.UpdateTimer(currentTimer)
            if (currentTimer <= 0)
                SpawnBoss();
        }
    }

    IEnumerator SpawnWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        currentWave++;
        UI.UpdateWaves(currentWave, wavesMax);
        waveStartTime = Time.time;

        // Если это последняя волна — вызываем появление босса и не спавним обычных врагов
        if (currentWave >= wavesMax && !bossSpawned)
        {
            SpawnBoss();
            yield break;
        }
        // Спавним врагов в случайных точках, стараясь не спавнить поверх предыдущего
        int count = Mathf.RoundToInt(enemiesPerWave * waveDifficulty);
        for (int i = 0; i < count; i++)
        {
            int maxAttempts = 10;
            int attempts = 0;
            int index = Random.Range(0, spawnPoints.Length);
            bool occupied = false;

            // Пытаемся найти точку, где предыдущий юнит отсутствует или разрушен
            while (attempts < maxAttempts)
            {
                occupied = false;
                if (lastSpawned != null && index >= 0 && index < lastSpawned.Length)
                {
                    var prev = lastSpawned[index];
                    if (prev != null)
                    {
                        if (prev.activeInHierarchy)
                        {
                            float dist = Vector3.Distance(prev.transform.position, spawnPoints[index].position);
                            if (dist < spawnCheckRadius) occupied = true;
                        }
                        else
                        {
                            // объект деактивирован/уничтожен — освобождаем ссылку
                            lastSpawned[index] = null;
                        }
                    }
                }

                if (!occupied) break;

                index = Random.Range(0, spawnPoints.Length);
                attempts++;
            }

            Vector3 spawnPos = spawnPoints[index].position;

            // Если все попытки не дали свободной точки, применяем небольшой оффсет, чтобы не спавнить в точности в том же месте
            if (occupied)
            {
                Vector3 offset = Random.insideUnitSphere * spawnOffsetRadius;
                offset.y = 0f;
                spawnPos += offset;
            }

            var enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            if (lastSpawned != null && index >= 0 && index < lastSpawned.Length)
                lastSpawned[index] = enemy;

            enemiesAlive++;

            yield return null; // минимальная пауза, чтобы не нагружать фреймы
        }
    }

    // Вызывается из EnemyStats.Die()
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

        // Быстрая зачистка — усиляем следующую волну
        if (clearTime < waveDuration * fastClearThreshold)
        {
            waveDifficulty *= waveStrengthMultiplier;
            // Позже: SoulManager.Instance.AddSouls(bonusSoulsOnFastClear)
            Debug.Log("Быстрая зачистка! Следующая волна сложнее.");
        }

        if (!bossSpawned)
            yield return StartCoroutine(SpawnWave());
    }

    void SpawnBoss()
    {
        bossSpawned = true;
        Debug.Log("Босс появился!");

        // Обнуляем таймер и скрываем его в UI
        currentTimer = 0f;
        if (UI != null) UI.UpdateTimer(0f);

        // Инстанцируем босса в указанной точке
        if (bossPrefab != null && bossSpawnPoint != null)
        {
            Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
        }
    }

    // Добавь этот вызов в EnemyStats.Die()
    // WaveManager wm = FindObjectOfType<WaveManager>();
    // if (wm != null) wm.OnEnemyDied();
}
