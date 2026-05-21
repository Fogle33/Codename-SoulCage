using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.down;
    private bool isDashing = false;
    private bool canDash = true;
    private float cooldownTimer = 0f;

    public float DashCooldownProgress
    {
        get
        {
            if (canDash) return 0f;
            if (isDashing) return 1f;
            return Mathf.Clamp01(cooldownTimer / dashCooldown);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        moveSpeed += PlayerUpgrades.SpeedBonus;
    }

    void Update()
    {
        if (!isDashing)
            rb.linearVelocity = moveInput * moveSpeed;

        if (!canDash && !isDashing)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                cooldownTimer = 0f;
                canDash = true;
            }
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        if (moveInput != Vector2.zero)
            lastMoveDirection = moveInput.normalized;
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash && !isDashing)
            StartCoroutine(DashCoroutine());
    }

    IEnumerator DashCoroutine()
    {
        isDashing = true;
        canDash = false;

        Vector2 dashDirection = lastMoveDirection;

        float timer = 0f;
        while (timer < dashDuration)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;
        cooldownTimer = dashCooldown;
    }
}