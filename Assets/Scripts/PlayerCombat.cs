using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    public GameObject meleeHitbox;
    public float attackCooldown = 0.5f;
    public float attackActiveDuration = 0.2f;

    private bool canAttack = true;
    private bool isAttacking = false;
    private bool isDisabled = false;
    private float cooldownTimer = 0f;
    private Vector2 mousePos;

    public float AttackCooldownProgress
    {
        get
        {
            if (canAttack) return 0f;
            if (isAttacking) return 1f;
            return Mathf.Clamp01(cooldownTimer / attackCooldown);
        }
    }
    void OnEnable()
    {
        isDisabled = false;
    }

    void Update()
    {
        // cooldown тикает всегда
        if (!canAttack && !isAttacking)
        {
            cooldownTimer -= Time.unscaledDeltaTime;
            if (cooldownTimer <= 0f)
            {
                cooldownTimer = 0f;
                canAttack = true;
            }
        }

        if (isAttacking) return; // только хитбокс замораживаем

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
        if (isDisabled) return;
        if (context.performed && canAttack)
            StartCoroutine(AttackCoroutine());
    }

    IEnumerator AttackCoroutine()
    {
        canAttack = false;
        isAttacking = true;
        meleeHitbox.SetActive(true);

        yield return new WaitForSecondsRealtime(attackActiveDuration);

        meleeHitbox.SetActive(false);
        isAttacking = false;
        cooldownTimer = attackCooldown;
    }

    public void ForceStopAttack()
    {
        isDisabled = true;
        StopAllCoroutines();
        if (meleeHitbox != null)
            meleeHitbox.SetActive(false);
        canAttack = true;
        isAttacking = false;
        cooldownTimer = 0f;
    }

    void OnDisable()
    {
        ForceStopAttack();
    }
}