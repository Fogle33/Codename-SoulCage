using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Спавн")]
    public GameObject enemyPrefab;          // Prefab врага
    public Transform[] spawnPoints;          // Точки спавна на арене
    public int enemiesPerWave = 5;              // Врагов в волне
    public float enemyWait = 1f; //Время задержки между спавнами
    public float spawnCheckRadius = 0.6f; // Радиус, по которому считаем, что предыдущий юнит всё ещё занимает точку
    public float spawnOffsetRadius = 0.5f; // Максимальный небольшой оффсет, если все точки заняты
    public float timeBetweenWaves = 3f;      // Пауза между волнами

    [Header("Таймер босса")]
    public float bossTimer = 120f;           // Секунд до появления босса
    private float currentTimer;
    private bool bossSpawned = false;

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

        StartCoroutine(SpawnWave());
    }

    void Update()
    {
        // Отсчёт таймера босса
        if (!bossSpawned)
        {
            currentTimer -= Time.deltaTime;
            // Позже здесь будет: UIManager.Instance.UpdateTimer(currentTimer)
            if (currentTimer <= 0)
                SpawnBoss();
        }
    }

    IEnumerator SpawnWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        currentWave++;
        waveStartTime = Time.time;

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
        // Позже: Instantiate(bossPrefab, ...)
    }

    // Добавь этот вызов в EnemyStats.Die()
    // WaveManager wm = FindObjectOfType<WaveManager>();
    // if (wm != null) wm.OnEnemyDied();
}
