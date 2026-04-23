using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    [Header("Спавн")]
    public GameObject enemyPrefab;          // Prefab врага
    public Transform[] spawnPoints;          // Точки спавна на арене
    public int enemiesPerWave = 5;              // Врагов в волне
    public float enemyWait = 1f; //Время задержки между спавнами
    public float spawnCheckRadius = 0.6f; // Радиус проверки занятости точки спавна
    public LayerMask spawnCheckMask; // Какие слои считать занятыми (например, слой врагов)
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

    void Start()
    {
        currentTimer = bossTimer;
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

        // Спавним врагов в случайных точках
        int count = Mathf.RoundToInt(enemiesPerWave * waveDifficulty);
        for (int i = 0; i < count; i++)
        {
            // Выбираем случайную точку спавна и проверяем, свободна ли она
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            int attempts = 0;
            const int maxAttempts = 6;
            // Пытаемся найти свободную точку среди spawnPoints
            while (Physics.CheckSphere(spawnPoint.position, spawnCheckRadius, spawnCheckMask) && attempts < maxAttempts)
            {
                spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                attempts++;
            }

            // Если после попыток точка всё ещё занята, ждём небольшой интервал и пробуем снова
            int waitLoops = 0;
            while (Physics.CheckSphere(spawnPoint.position, spawnCheckRadius, spawnCheckMask) && waitLoops < 10)
            {
                yield return new WaitForSeconds(enemyWait);
                waitLoops++;
            }

            Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            enemiesAlive++;

            // Задержка между созданием противников, чтобы они не появлялись одновременно
            yield return new WaitForSeconds(enemyWait);
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
