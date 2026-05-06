using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP;
    private ArenaUI ui;

    void Awake()
    {
        currentHP = maxHP;
        ui = FindObjectOfType<ArenaUI>();
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        if (ui) ui.UpdateHP(currentHP, maxHP);
        if (currentHP <= 0) Die();
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        if (ui) ui.UpdateHP(currentHP, maxHP);
    }

    void Die()
    {
        DeathScreen.Instance?.Show();
    }
}