using UnityEngine;
using System.Collections;

public class AbsorberBossAI : MonoBehaviour
{
    [Header("Движение")]
    public float moveSpeed = 1.5f;
    public float fleeDistance = 4f;
    public float wanderRadius = 2f;
    public float wanderInterval = 2f;

    [Header("Поглощение")]
    public float absorbRadius = 3f;
    public float absorbInterval = 2f;
    public float healPerAbsorb = 20f;

    [Header("Field Attack")]
    public GameObject fieldPrefab;
    public float fieldCooldown = 6f;
    public float preAttackDelay = 1.2f;
    public float fieldDuration = 2f;
    public float fieldDamage = 20f;

    [Header("Phase 2")]
    public float phase2HPPercent = 0.5f;
    public float phase2FieldCooldown = 3f;
    public float phase2SpeedMultiplier = 1.8f;
    public float dashCooldown = 3f;

    [HideInInspector] public bool playerInField = false;

    private Transform player;
    private Rigidbody2D rb;
    private EnemyStats stats;
    private Transform[] corners;

    private bool playerNear = false;
    private bool inCorner = false;
    private Transform currentCorner;
    private Transform lastDashTarget;
    private float lastDashEndTime;

    private Vector2 wanderTarget;
    private float lastWanderTime;
    private float lastAbsorbTime;
    private float lastFieldTime = -Mathf.Infinity;
    private float lastDashTime = -Mathf.Infinity;
    private bool isPreparing = false;
    private bool isDashing = false;
    private bool isPhase2 = false;
    private GameObject currentField;
    private float fleeExitTime = -999f;
    public float reactCooldown = 1.5f;
    public void OnEnterCorner(Transform corner) { inCorner = true; currentCorner = corner; }
    public void OnExitCorner() { inCorner = false; currentCorner = null; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<EnemyStats>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        var objs = GameObject.FindGameObjectsWithTag("Corner");
        corners = new Transform[objs.Length];
        for (int i = 0; i < objs.Length; i++) corners[i] = objs[i].transform;

        wanderTarget = rb.position;
        Debug.Log($"[Boss] Corners: {corners.Length}");
    }

    void FixedUpdate()
    {
        if (player == null || isDashing) return;

        float dist = Vector2.Distance(rb.position, player.position);
        bool wasNear = playerNear;
        playerNear = dist < fleeDistance;

        // Вышел из зоны — запоминаем время
        if (wasNear && !playerNear)
            fleeExitTime = Time.time;

        // Cooldown после выхода — не реагируем сразу
        bool canReact = Time.time >= fleeExitTime + reactCooldown;
        bool shouldFlee = playerNear && canReact;

        if (playerNear && !inCorner)
        {
            Transform target = GetSafestCorner();
            Vector2 dir = target != null
                ? ((Vector2)target.position - rb.position).normalized
                : (rb.position - (Vector2)player.position).normalized;
            rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
        }
        else if (!playerNear)
        {
            if (Time.time >= lastWanderTime + wanderInterval ||
                Vector2.Distance(rb.position, wanderTarget) < 0.2f)
            {
                Vector2 center = Vector2.zero;
                Transform nearestCorner = GetNearestCorner();
                if (nearestCorner != null)
                {
                    // Точка между центром и противоположной стороной от угла
                    Vector2 awayFromCorner = (Vector2.zero - (Vector2)nearestCorner.position).normalized;
                    center = Vector2.zero + awayFromCorner * (wanderRadius * 0.5f);
                }
                wanderTarget = center + Random.insideUnitCircle * wanderRadius;
                lastWanderTime = Time.time;
            }
            Vector2 dir = (wanderTarget - rb.position).normalized;
            rb.MovePosition(rb.position + dir * moveSpeed * 0.5f * Time.fixedDeltaTime);
        }
        // inCorner && playerNear — стоим, ждём рывка
    }

    void Update()
    {
        if (player == null) return;

        if (!isPhase2 && stats.currentHP / stats.maxHP <= phase2HPPercent)
            EnterPhase2();

        if (Time.time >= lastAbsorbTime + absorbInterval)
        {
            AbsorbNearby();
            lastAbsorbTime = Time.time;
        }

        float cd = isPhase2 ? phase2FieldCooldown : fieldCooldown;
        if (!isPreparing && !isDashing && Time.time >= lastFieldTime + cd)
            StartCoroutine(FieldAttackRoutine());

        if (!isDashing && !isPreparing &&
    playerNear && inCorner && Time.time >= lastDashTime + dashCooldown)
            StartCoroutine(DashToFarthestCorner());
    }

    void EnterPhase2()
    {
        isPhase2 = true;
        moveSpeed *= phase2SpeedMultiplier;
    }

    Transform GetNearestCorner()
    {
        if (corners == null || corners.Length == 0) return null;
        Transform nearest = null;
        float min = float.MaxValue;
        foreach (var c in corners)
        {
            if (c == lastDashTarget && Time.time - lastDashEndTime < dashCooldown) continue;
            float d = Vector2.Distance(rb.position, c.position);
            if (d < min) { min = d; nearest = c; }
        }
        return nearest;
    }
    Transform GetSafestCorner()
    {
        if (corners == null || corners.Length == 0) return null;
        Transform best = null;
        float bestScore = float.MinValue;

        Vector2 toPlayer = ((Vector2)player.position - rb.position).normalized;

        foreach (var c in corners)
        {
            if (c == lastDashTarget && Time.time - lastDashEndTime < dashCooldown) continue;
            Vector2 toCorner = ((Vector2)c.position - rb.position).normalized;
            float dot = Vector2.Dot(toCorner, toPlayer);
            float dist = Vector2.Distance(rb.position, c.position);
            // dot = -1: угол строго за спиной у игрока = лучший
            // dist штраф небольшой чтобы не бежал в самый дальний угол
            float score = -dot - dist * 0.05f;
            if (score > bestScore) { bestScore = score; best = c; }
        }
        return best;
    }
    Transform GetNearestCornerExcluding(Transform exclude)
    {
        if (corners == null || corners.Length == 0) return null;
        Transform nearest = null;
        float min = float.MaxValue;
        foreach (var c in corners)
        {
            if (c == exclude) continue;
            if (c == lastDashTarget && Time.time - lastDashEndTime < dashCooldown) continue;
            float d = Vector2.Distance(rb.position, c.position);
            if (d < min) { min = d; nearest = c; }
        }
        return nearest;
    }

    void AbsorbNearby()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, absorbRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy") && hit.gameObject != gameObject)
            {
                stats.currentHP = Mathf.Min(stats.currentHP + healPerAbsorb, stats.maxHP);
                Destroy(hit.gameObject);
            }
        }
    }

    IEnumerator FieldAttackRoutine()
    {
        isPreparing = true;
        playerInField = false;

        yield return new WaitForSeconds(preAttackDelay);

        if (fieldPrefab != null && player != null)
        {
            currentField = Instantiate(fieldPrefab, player.position, Quaternion.identity);
            var fa = currentField.GetComponent<FieldAttack>();
            if (fa != null) fa.boss = this;
        }

        yield return new WaitForSeconds(fieldDuration);

        bool hit = false;
        if (currentField != null)
        {
            var col = currentField.GetComponent<Collider2D>();
            var pCol = player?.GetComponent<Collider2D>();
            if (col != null && pCol != null)
                hit = col.bounds.Intersects(pCol.bounds);
            Destroy(currentField);
            currentField = null;
        }

        if (hit) player?.GetComponent<PlayerStats>()?.TakeDamage(fieldDamage);

        lastFieldTime = Time.time;
        isPreparing = false;
    }

    IEnumerator DashToFarthestCorner()
    {
        isDashing = true;
        lastDashTime = Time.time;

        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        Transform target = null;
        float max = 0f;
        foreach (var c in corners)
        {
            if (c == currentCorner) continue;
            float d = Vector2.Distance(rb.position, c.position);
            if (d > max) { max = d; target = c; }
        }

        if (target == null)
        {
            if (col) col.enabled = true;
            isDashing = false;
            yield break;
        }

        Vector2 dir = ((Vector2)target.position - rb.position).normalized;
        float timeout = 1.5f;
        float t = 0f;
        while (Vector2.Distance(rb.position, target.position) > 0.5f && t < timeout)
        {
            rb.MovePosition(rb.position + dir * 25f * Time.fixedDeltaTime);
            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        lastDashTarget = target;
        lastDashEndTime = Time.time;
        if (col) col.enabled = true;
        isDashing = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, absorbRadius);
    }
}