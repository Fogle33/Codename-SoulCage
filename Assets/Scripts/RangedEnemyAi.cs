using UnityEngine;

public class RangedEnemyAI : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float attackRange = 6f;
    public float fleeRange = 3f;
    public float damage = 15f;
    public float attackCooldown = 2f;
    public GameObject projectilePrefab;
    public float reactCooldown = 1f;

    private Transform player;
    private Rigidbody2D rb;
    private float lastAttackTime;
    private float fleeExitTime = -999f;
    private bool wasFleeing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;
        float distance = Vector2.Distance(transform.position, player.position);

        bool isFleeing = distance < fleeRange;

        if (wasFleeing && !isFleeing)
            fleeExitTime = Time.time;

        wasFleeing = isFleeing;

        bool cooldownActive = Time.time < fleeExitTime + reactCooldown;

        if (isFleeing && !cooldownActive)
        {
            Vector2 fleeDir = ((Vector2)transform.position - (Vector2)player.position).normalized;
            rb.MovePosition(rb.position + fleeDir * moveSpeed * Time.fixedDeltaTime);
        }
        else if (distance > attackRange && !cooldownActive)
        {
            Vector2 dir = ((Vector2)player.position - rb.position).normalized;
            rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
        }
        // cooldownActive — просто стоит на месте
    }

    void Update()
    {
        if (player == null) return;
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    void Attack()
    {
        if (projectilePrefab == null) return;
        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        proj.GetComponent<Rigidbody2D>().linearVelocity = dir * 8f;
    }
}