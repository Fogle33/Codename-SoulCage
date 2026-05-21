using UnityEngine;

public class CornerZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        other.GetComponent<AbsorberBossAI>()?.OnEnterCorner(transform);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        other.GetComponent<AbsorberBossAI>()?.OnExitCorner();
    }
}