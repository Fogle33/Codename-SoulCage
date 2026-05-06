using UnityEngine;

public class AbsorberBossAI : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float absorbRadius = 3f;     // Радиус всасывания треша
    public float absorbInterval = 2f;   // Как часто всасывает
    public float healPerAbsorb = 20f;   // Сколько HP восстанавливает за каждого врага

    private Transform player;
    private Rigidbody2D rb;
    private EnemyStats stats;
    private float lastAbsorbTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<EnemyStats>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;
        // Медленно движется к игроку
        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
    }

    void Update()
    {
        // Периодически всасывает треш
        if (Time.time >= lastAbsorbTime + absorbInterval)
        {
            AbsorbNearbyTrash();
            lastAbsorbTime = Time.time;
        }
    }

    void AbsorbNearbyTrash()
    {
        // Ищем всех врагов в радиусе кроме себя
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, absorbRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy") && hit.gameObject != gameObject)
            {
                // Уничтожаем врага и хилимся
                stats.currentHP = Mathf.Min(stats.currentHP + healPerAbsorb, stats.maxHP);
                Destroy(hit.gameObject);
                Debug.Log("Boss absorbed an enemy!");
            }
        }
    }

    // Визуализация радиуса в редакторе (только для отладки)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, absorbRadius);
    }
}
