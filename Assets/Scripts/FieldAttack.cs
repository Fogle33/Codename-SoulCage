using UnityEngine;

public class FieldAttack : MonoBehaviour
{
    // Ссылка на босса — устанавливается при создании префаба
    public AbsorberBossAI boss;

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            boss.playerInField = true;
        }
    }
}