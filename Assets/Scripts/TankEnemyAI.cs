using UnityEngine;
using System.Collections;

public class TankEnemyAI : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    public float damage = 15f;
    public float attackCooldown = 1.5f;
    public float chargeSpeed = 5f;
    public float chargeDistance = 4f;
    public float chargeDuration = 0.4f;

    private Transform player;
    private Rigidbody2D rb;
    private float lastAttackTime;
    private float lastChargeTime;
    private bool isCharging = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        if (player == null || isCharging) return;
        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
    }

    void Update()
    {
        if (player == null) return;
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist < chargeDistance && Time.time >= lastChargeTime + attackCooldown && !isCharging)
            StartCoroutine(Charge());
    }

    IEnumerator Charge()
    {
        isCharging = true;
        lastChargeTime = Time.time;
        Vector2 chargeDir = ((Vector2)player.position - rb.position).normalized;
        float timer = 0f;
        while (timer < chargeDuration)
        {
            rb.MovePosition(rb.position + chargeDir * chargeSpeed * Time.fixedDeltaTime);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        isCharging = false;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                collision.gameObject.GetComponent<PlayerStats>()?.TakeDamage(damage);
                lastAttackTime = Time.time;
            }
        }
    }
}