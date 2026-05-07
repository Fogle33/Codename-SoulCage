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
    public float phase2HPPercent = 0.4f;
    public float phase2FieldCooldown = 3f;
    public float phase2MoveSpeedMultiplier = 1.3f;

    [HideInInspector] public bool playerInField = false;

    private Transform player;
    private Rigidbody2D rb;
    private EnemyStats stats;
    private Transform[] corners;
    private Vector2 wanderTarget;
    private float lastWanderTime;
    private float lastAbsorbTime;
    private float lastFieldTime = -Mathf.Infinity;
    private float lastDashTime = -Mathf.Infinity;
    private bool isPreparing = false;
    private bool isDashing = false;
    private bool isPhase2 = false;
    private GameObject currentField;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<EnemyStats>();

        player = GameObject.FindGameObjectWithTag("Player").transform;

        GameObject[] cornerObjects = GameObject.FindGameObjectsWithTag("Corner");
        corners = new Transform[cornerObjects.Length];
        for (int i = 0; i < cornerObjects.Length; i++)
            corners[i] = cornerObjects[i].transform;

        wanderTarget = rb.position;
    }

    void FixedUpdate()
    {
        if (player == null || isDashing) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist < fleeDistance)
        {
            Vector2 away = ((Vector2)transform.position - (Vector2)player.position).normalized;
            rb.MovePosition(rb.position + away * moveSpeed * Time.fixedDeltaTime);
        }
        else
        {
            if (Time.time >= lastWanderTime + wanderInterval ||
                Vector2.Distance(rb.position, wanderTarget) < 0.2f)
            {
                wanderTarget = rb.position + Random.insideUnitCircle * wanderRadius;
                lastWanderTime = Time.time;
            }

            Vector2 wanderDir = (wanderTarget - rb.position).normalized;
            rb.MovePosition(rb.position + wanderDir * (moveSpeed * 0.5f) * Time.fixedDeltaTime);
        }
    }

    void Update()
    {
        if (player == null) return;

        if (!isPhase2 && stats.currentHP / stats.maxHP <= phase2HPPercent)
            EnterPhase2();

        if (Time.time >= lastAbsorbTime + absorbInterval)
        {
            AbsorbNearbyTrash();
            lastAbsorbTime = Time.time;
        }

        float currentFieldCooldown = isPhase2 ? phase2FieldCooldown : fieldCooldown;
        if (Time.time >= lastFieldTime + currentFieldCooldown && !isPreparing && !isDashing)
            StartCoroutine(PrepareAndSpawnField());

        if (isPhase2 && Time.time >= lastDashTime + 5f && !isDashing && !isPreparing)
            StartCoroutine(DashToCorner());
    }

    void EnterPhase2()
    {
        isPhase2 = true;
        moveSpeed *= phase2MoveSpeedMultiplier;
    }

    void AbsorbNearbyTrash()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, absorbRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy") && hit.gameObject != gameObject)
            {
                stats.currentHP = Mathf.Min(stats.currentHP + healPerAbsorb, stats.maxHP);
                Destroy(hit.gameObject);
            }
        }
    }

    IEnumerator PrepareAndSpawnField()
    {
        isPreparing = true;
        playerInField = false;

        if (preAttackDelay > 0f)
            yield return new WaitForSeconds(preAttackDelay);

        if (fieldPrefab != null && player != null)
        {
            Vector3 spawnPos = player.position;
            currentField = Instantiate(fieldPrefab, spawnPos, Quaternion.identity);
            currentField.transform.SetParent(null);
            FieldAttack fa = currentField.GetComponent<FieldAttack>();
            if (fa != null) fa.boss = this;
        }

        yield return new WaitForSeconds(fieldDuration);

        bool hitPlayer = false;
        if (currentField != null)
        {
            Collider2D col = currentField.GetComponent<Collider2D>();
            if (col != null && player != null)
            {
                Collider2D playerCol = player.GetComponent<Collider2D>();
                if (playerCol != null)
                    hitPlayer = col.bounds.Intersects(playerCol.bounds);
            }
        }

        if (currentField != null)
        {
            Destroy(currentField);
            currentField = null;
        }

        if (hitPlayer)
            player.GetComponent<PlayerStats>()?.TakeDamage(fieldDamage);

        lastFieldTime = Time.time;
        isPreparing = false;
    }

    IEnumerator DashToCorner()
    {
        if (corners == null || corners.Length == 0) yield break;

        isDashing = true;
        lastDashTime = Time.time;

        Transform bestCorner = corners[0];
        float bestDist = 0f;
        foreach (var corner in corners)
        {
            float d = Vector2.Distance(corner.position, player.position);
            if (d > bestDist) { bestDist = d; bestCorner = corner; }
        }

        Vector2 dashDir = ((Vector2)bestCorner.position - rb.position).normalized;
        float t = 0f;
        while (t < 0.5f)
        {
            rb.MovePosition(rb.position + dashDir * 12f * Time.fixedDeltaTime);
            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, absorbRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);
    }
}