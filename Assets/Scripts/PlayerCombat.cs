using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    public GameObject meleeHitbox;
    public float attackCooldown = 0.5f;
    private bool canAttack = true;
    private bool isDisabled = false; // Новый флаг
    private Vector2 mousePos;

    void Update()
    {
        mousePos = Mouse.current.position.ReadValue();
        Vector3 worldMouse = Camera.main.ScreenToWorldPoint(mousePos);
        worldMouse.z = 0f;

        Vector2 direction = ((Vector2)worldMouse - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        meleeHitbox.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        meleeHitbox.transform.localPosition = direction * 0.8f;
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (isDisabled) return; // Блокируем атаку
        if (context.performed && canAttack)
            StartCoroutine(AttackCoroutine());
    }

    IEnumerator AttackCoroutine()
    {
        canAttack = false;
        meleeHitbox.SetActive(true);

        yield return new WaitForSecondsRealtime(0.2f);

        meleeHitbox.SetActive(false);

        yield return new WaitForSecondsRealtime(attackCooldown);
        canAttack = true;
    }

    public void ForceStopAttack()
    {
        isDisabled = true;
        StopAllCoroutines();
        if (meleeHitbox != null)
            meleeHitbox.SetActive(false);
        canAttack = true;
    }

    void OnDisable()
    {
        ForceStopAttack();
    }
}