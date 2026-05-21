using UnityEngine;
using System.Collections;

public class FinalBossAI : MonoBehaviour
{
    [Header("Движение")]
    public float moveSpeed = 0.8f;

    [Header("Атака")]
    public float damage = 25f;
    public float attackCooldown = 3f;
    public float attackRange = 8f;
    public float telegraphDuration = 1.5f;
    public float attackZoneOffset = 3f;

    [Header("Фаза 2")]
    public float phase2HPPercent = 0.5f;
    public float phase2AttackCooldown = 4f;
    public float phase2DashSpeed = 22f;
    public float phase2AttackZoneOffset = 5f;

    public GameObject attackZoneVisual;

    private Transform player;
    private Rigidbody2D rb;
    private EnemyStats stats;
    private SpriteRenderer sr;

    private bool isAttacking = false;
    private bool isPhase2 = false;
    private bool enteringPhase2 = false;

    private float lastAttackTime = -Mathf.Infinity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<EnemyStats>();
        sr = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (attackZoneVisual) attackZoneVisual.SetActive(false);
    }

    void FixedUpdate()
    {
        if (player == null || isAttacking || enteringPhase2) return;
        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
    }

    void Update()
    {
        if (player == null || enteringPhase2) return;

        if (!isPhase2 && stats.currentHP / stats.maxHP <= phase2HPPercent)
            StartCoroutine(EnterPhase2());

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > attackRange || isAttacking) return;

        float cd = isPhase2 ? phase2AttackCooldown : attackCooldown;
        if (Time.time >= lastAttackTime + cd)
        {
            if (isPhase2)
                StartCoroutine(Phase2Attack());
            else
                StartCoroutine(TelegraphAttack());
        }
    }

    IEnumerator EnterPhase2()
    {
        isPhase2 = true;
        enteringPhase2 = true;
        moveSpeed *= 1.5f;

        if (sr)
        {
            Color original = sr.color;
            for (int i = 0; i < 8; i++)
            {
                sr.color = new Color(1f, 0.1f, 0.1f, 1f);
                yield return new WaitForSeconds(0.12f);
                sr.color = original;
                yield return new WaitForSeconds(0.12f);
            }
        }

        enteringPhase2 = false;
    }

    IEnumerator TelegraphAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        Vector2 attackDir = ((Vector2)player.position - rb.position).normalized;
        float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;

        if (attackZoneVisual)
        {
            attackZoneVisual.transform.position = rb.position +
                attackDir * (attackZoneOffset + transform.localScale.x * 0.5f);
            attackZoneVisual.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            var zoneSr = attackZoneVisual.GetComponent<SpriteRenderer>();
            if (zoneSr) zoneSr.color = new Color(1f, 0.1f, 0.1f, 0.3f);
            attackZoneVisual.SetActive(true);
        }

        yield return new WaitForSeconds(telegraphDuration);

        if (attackZoneVisual)
        {
            var zoneSr = attackZoneVisual.GetComponent<SpriteRenderer>();
            if (zoneSr) zoneSr.color = new Color(1f, 0.1f, 0.1f, 0.85f);
        }

        // Урон зоной
        if (attackZoneVisual != null)
        {
            var boxCol = attackZoneVisual.GetComponent<BoxCollider2D>();
            if (boxCol != null)
            {
                Vector2 size = Vector2.Scale(boxCol.size, attackZoneVisual.transform.lossyScale);
                float angleA = attackZoneVisual.transform.eulerAngles.z;
                var hits = Physics2D.OverlapBoxAll(
                    attackZoneVisual.transform.position, size, angleA);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Player"))
                    {
                        hit.GetComponentInParent<PlayerStats>()?.TakeDamage(damage);
                        break;
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.3f);
        if (attackZoneVisual) attackZoneVisual.SetActive(false);
        isAttacking = false;
    }

    IEnumerator Phase2Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        Vector2 attackDir = ((Vector2)player.position - rb.position).normalized;
        float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;

        if (attackZoneVisual)
        {
            attackZoneVisual.transform.position = rb.position +
                attackDir * (phase2AttackZoneOffset + transform.localScale.x * 0.5f);
            attackZoneVisual.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            var zoneSr = attackZoneVisual.GetComponent<SpriteRenderer>();
            if (zoneSr) zoneSr.color = new Color(1f, 0.1f, 0.1f, 0.3f);
            attackZoneVisual.SetActive(true);
        }

        yield return new WaitForSeconds(telegraphDuration);

        if (attackZoneVisual) attackZoneVisual.SetActive(false);

        bool hitOnce = false;
        float t = 0f;
        float hitRadius = transform.localScale.x * 0.8f; // радиус под размер босса
        while (t < 0.5f)
        {
            rb.MovePosition(rb.position + attackDir * phase2DashSpeed * Time.fixedDeltaTime);

            if (!hitOnce)
            {
                var hits = Physics2D.OverlapCircleAll(rb.position, hitRadius);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Player"))
                    {
                        hit.GetComponentInParent<PlayerStats>()?.TakeDamage(damage);
                        hitOnce = true;
                        break;
                    }
                }
            }

            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isAttacking = false;
    }
}