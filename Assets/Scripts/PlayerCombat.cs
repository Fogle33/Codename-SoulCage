using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    public GameObject meleeHitbox;
    public float attackCooldown = 0.5f;
    private bool canAttack = true;
    private Vector2 mousePos;

    void Update()
    {
        // Поворот хитбокса к мыши каждый кадр
        mousePos = Mouse.current.position.ReadValue();
        Vector3 worldMouse = Camera.main.ScreenToWorldPoint(mousePos);
        worldMouse.z = 0f;

        Vector2 direction = ((Vector2)worldMouse - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        meleeHitbox.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed && canAttack)
            StartCoroutine(AttackCoroutine());
    }

    IEnumerator AttackCoroutine()
    {
        canAttack = false;
        meleeHitbox.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        meleeHitbox.SetActive(false);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}