using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP;

    void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        // Позже здесь будет обновление UI
        if (currentHP <= 0)
            Die();
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP); // Не превышаем максимум
    }

    void Die()
    {
        Debug.Log("Player died");
        // Позже здесь будет: GameManager.Instance.OnPlayerDeath()
    }
}

