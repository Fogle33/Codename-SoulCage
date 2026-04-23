// MeleeHitbox.cs — на дочернем объекте
using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    public float damage = 10f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // позже здесь будет other.GetComponent<EnemyStats>().TakeDamage(damage);
            Debug.Log("Hit: " + other.name);
        }
    }
}