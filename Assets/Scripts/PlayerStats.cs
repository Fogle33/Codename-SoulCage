using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP;
    public ArenaUI UI;

    void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        UI.UpdateHP(currentHP, maxHP);
        if (currentHP <= 0)
            Die();
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP); // Не превышаем максимум
        UI.UpdateHP(currentHP, maxHP);
    }

    void Die()
    {
        Debug.Log("Player died");
        // Позже здесь будет: GameManager.Instance.OnPlayerDeath()
    }
}

