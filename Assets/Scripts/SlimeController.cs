using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : MonoBehaviour {
    // Public variables
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float highJumpMultiplier = 1.5f;
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public Color highJumpColor = Color.yellow;
    public Color dashColor = Color.cyan;
    public float squashSpeed = 10f;

    // Private variables
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float horizontalInput;
    private bool isGrounded;
    private bool isDashing = false;
    private Vector3 originalScale;
    private bool wasGrounded;
    private Coroutine squashCoroutine;

    // ===== BIẾN MỚI ĐỂ QUẢN LÝ LƯỚT TRÊN KHÔNG =====
    private bool canAirDash;

    // ===== BIẾN MỚI ĐỂ XỬ LÝ QUAY MẶT =====
    private bool isFacingRight = true;

    // Hệ thống năng lực
    private List<AbilityType> unlockedAbilities = new List<AbilityType>();

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    void Update() {
        if (isDashing) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // ===== LOGIC MỚI: HỒI LẠI CÚ LƯỚT KHI CHẠM ĐẤT =====
        if (isGrounded)
        {
            canAirDash = true;
        }

        // Xử lý nhảy
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        // ===== LOGIC MỚI: KIỂM TRA ĐIỀU KIỆN LƯỚT =====
        // Điều kiện: Có năng lực Lướt VÀ (đang trên mặt đất HOẶC còn lượt lướt trên không)
        if (Input.GetKeyDown(KeyCode.LeftShift) && HasAbility(AbilityType.Dash) && (isGrounded || canAirDash))
        {
            StartCoroutine(DashCoroutine());
        }

        // Xử lý hiệu ứng tiếp đất
        if (isGrounded && !wasGrounded)
        {
            HandleSquashAndStretch(0.8f, 1.2f);
        }
        wasGrounded = isGrounded;
    }

    void FixedUpdate() {
        if (isDashing) return;

        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

        // ===== LOGIC QUAY MẶT MỚI =====
        // Nếu đang đi sang trái và mặt đang quay phải -> quay mặt lại
        if (horizontalInput < 0 && isFacingRight)
        {
            Flip();
        }
        // Nếu đang đi sang phải và mặt đang quay trái -> quay mặt lại
        else if (horizontalInput > 0 && !isFacingRight)
        {
            Flip();
        }
    }

    // ===== HÀM QUAY MẶT MỚI =====
    void Flip() {
        isFacingRight = !isFacingRight;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1; // Lật scale theo trục X
        transform.localScale = newScale;
    }

    // ===== Các hàm xử lý chính (không thay đổi nhiều) =====
    void Jump() {
        float currentJumpForce = jumpForce;
        if (HasAbility(AbilityType.HighJump))
        {
            currentJumpForce *= highJumpMultiplier;
        }
        rb.velocity = new Vector2(rb.velocity.x, currentJumpForce);
        HandleSquashAndStretch(1.2f, 0.8f);
    }

    IEnumerator DashCoroutine() {
        // ===== LOGIC MỚI: TIÊU HAO LƯỢT LƯỚT TRÊN KHÔNG =====
        if (!isGrounded)
        {
            canAirDash = false; // Dùng mất lượt lướt trên không
        }

        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        float dashDirection = isFacingRight ? 1 : -1;
        rb.velocity = new Vector2(dashDirection * dashForce, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
    }


    private void OnCollisionEnter2D(Collision2D collision) {
        // ... Hàm này giữ nguyên ...
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.contacts[0].normal.y > 0.5f)
            {
                AbilityGrant abilityGrant = collision.gameObject.GetComponent<AbilityGrant>();
                if (abilityGrant != null)
                {
                    Absorb(abilityGrant.abilityToGrant, collision.gameObject);
                }
            }
        }
    }

    void Absorb(AbilityType ability, GameObject enemy) {
        // ... Hàm này giữ nguyên ...
        Debug.Log("Hấp thụ năng lực: " + ability.ToString());
        Destroy(enemy);

        if (!HasAbility(ability))
        {
            unlockedAbilities.Add(ability);

            if (ability == AbilityType.HighJump) spriteRenderer.color = highJumpColor;
            if (ability == AbilityType.Dash) spriteRenderer.color = dashColor;
        }
    }

    bool HasAbility(AbilityType ability) {
        return unlockedAbilities.Contains(ability);
    }

    void HandleSquashAndStretch(float yScale, float xScale) {
        // ... Hàm này giữ nguyên ...
        if (squashCoroutine != null)
        {
            StopCoroutine(squashCoroutine);
        }
        squashCoroutine = StartCoroutine(SquashAndStretchCoroutine(yScale, xScale));
    }

    IEnumerator SquashAndStretchCoroutine(float yScale, float xScale) {
        // ... Hàm này giữ nguyên ...
        Vector3 targetScale = new Vector3(originalScale.x * Mathf.Abs(transform.localScale.x) * xScale, originalScale.y * yScale, originalScale.z);
        // Chỉnh lại một chút để nó không bị lỗi khi scale đang âm
        targetScale.x = originalScale.x * (isFacingRight ? 1 : -1) * xScale;

        Vector3 currentScale = transform.localScale;
        Vector3 startScaleForLerp = currentScale;

        float t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime * squashSpeed;
            transform.localScale = Vector3.Lerp(startScaleForLerp, targetScale, t);
            yield return null;
        }
        transform.localScale = targetScale;

        t = 0;
        startScaleForLerp = transform.localScale;
        Vector3 finalOriginalScale = new Vector3(originalScale.x * (isFacingRight ? 1 : -1), originalScale.y, originalScale.z);

        while (t < 1.0f)
        {
            t += Time.deltaTime * squashSpeed;
            transform.localScale = Vector3.Lerp(startScaleForLerp, finalOriginalScale, t);
            yield return null;
        }
        transform.localScale = finalOriginalScale;
    }
}