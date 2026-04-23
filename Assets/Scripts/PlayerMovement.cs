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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!isDashing)
            rb.linearVelocity = moveInput * moveSpeed;
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

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}