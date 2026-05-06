using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float damage = 15f;

    void Start() => Destroy(gameObject, 4f);

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerStats>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
        if (other.CompareTag("Wall"))
            Destroy(gameObject);
    }
}