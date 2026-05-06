using UnityEngine;

public class RangedEnemyAI : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float attackRange = 6f;      // Дистанция с которой атакует
    public float fleeRange = 3f;        // Дистанция с которой убегает
    public float damage = 15f;
    public float attackCooldown = 2f;
    public GameObject projectilePrefab; // Снаряд — создать отдельно

    private Transform player;
    private Rigidbody2D rb;
    private float lastAttackTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < fleeRange)
        {
            // Убегаем если игрок слишком близко
            Vector2 fleeDirection = ((Vector2)transform.position - (Vector2)player.position).normalized;
            rb.MovePosition(rb.position + fleeDirection * moveSpeed * Time.fixedDeltaTime);
        }
        else if (distance > attackRange)
        {
            // Приближаемся если слишком далеко
            Vector2 direction = ((Vector2)player.position - rb.position).normalized;
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
        }
        // Если в диапазоне — стоим и стреляем

        // Атака по кулдауну
        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    void Attack()
    {
        if (projectilePrefab == null) return;
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        proj.GetComponent<Rigidbody2D>().linearVelocity = direction * 8f;
    }
}
