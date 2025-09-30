using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : MonoBehaviour {
    // Public variables
    public float moveSpeed = 5f;
    public float jumpForce = 6f;
    public float highJumpMultiplier = 1.5f;
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    // ===== CÁC BIẾN MỚI CHO LEO TƯỜNG =====
    public Transform wallCheck; // Điểm kiểm tra tường
    public float wallSlidingSpeed = 2f; // Tốc độ trượt tường
    public Vector2 wallJumpForce = new Vector2(8f, 12f); // Lực nhảy tường (ngang, dọc)

    public Transform groundCheck;
    public LayerMask groundLayer;
    public Color highJumpColor = Color.yellow;
    public Color dashColor = Color.cyan;
    public Color wallClimbColor = Color.green; // Màu cho năng lực leo tường
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

    // ===== CÁC BIẾN MỚI CHO LEO TƯỜNG =====
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool canWallJump;
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
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, 0.2f, groundLayer);

        // Xác định "cơ hội" nhảy tường
        if (HasAbility(AbilityType.WallClimb) && isTouchingWall && !isGrounded)
        {
            canWallJump = true;
        } else
        {
            canWallJump = false;
        }

        // Xác định trạng thái trượt tường
        isWallSliding = canWallJump && ((isFacingRight && horizontalInput > 0) || (!isFacingRight && horizontalInput < 0));

        // ===== LOGIC HỒI LƯỚT ĐÃ CẬP NHẬT =====
        // Hồi lại cú lướt khi CHẠM ĐẤT hoặc KHI CÓ THỂ NHẢY TƯỜNG
        if (isGrounded || canWallJump)
        {
            canAirDash = true;
        }

        // Xử lý nhảy
        if (Input.GetButtonDown("Jump"))
        {
            if (canWallJump)
            {
                Jump(true);
            } else if (isGrounded)
            {
                Jump(false);
            }
        }

        // Xử lý lướt
        if (Input.GetKeyDown(KeyCode.LeftShift) && HasAbility(AbilityType.Dash) && (isGrounded || canAirDash))
        {
            StartCoroutine(DashCoroutine());
        }

        if (isGrounded && !wasGrounded)
        {
            HandleSquashAndStretch(0.8f, 1.2f);
        }
        wasGrounded = isGrounded;
    }

    void FixedUpdate() {
        if (isDashing) return;

        // Chỉ áp dụng hiệu ứng trượt khi isWallSliding là true
        if (isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        } else
        {
            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        }

        if (!isWallSliding)
        {
            if (horizontalInput < 0 && isFacingRight)
            {
                Flip();
            } else if (horizontalInput > 0 && !isFacingRight)
            {
                Flip();
            }
        }
    }
    // ===== HÀM JUMP ĐÃ ĐƯỢC CẢI TIẾN =====
    void Jump(bool isWallJumping) {
        if (isWallJumping)
        {
            // Cú nhảy tường LUÔN đẩy ra xa và lật người lại.
            // Điều này tạo ra cảm giác dứt khoát và có kiểm soát.
            rb.velocity = new Vector2(wallJumpForce.x * (isFacingRight ? -1 : 1), wallJumpForce.y);
            Flip();
        } else
        {
            float currentJumpForce = jumpForce;
            if (HasAbility(AbilityType.HighJump))
            {
                currentJumpForce *= highJumpMultiplier;
            }
            rb.velocity = new Vector2(rb.velocity.x, currentJumpForce);
        }
        HandleSquashAndStretch(1.2f, 0.8f);
    }


    // ===== HÀM QUAY MẶT MỚI =====
    void Flip() {
        isFacingRight = !isFacingRight;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1; // Lật scale theo trục X
        transform.localScale = newScale;
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
        Debug.Log("Hấp thụ năng lực: " + ability.ToString());
        Destroy(enemy);

        if (!HasAbility(ability))
        {
            unlockedAbilities.Add(ability);

            if (ability == AbilityType.HighJump) spriteRenderer.color = highJumpColor;
            if (ability == AbilityType.Dash) spriteRenderer.color = dashColor;
            if (ability == AbilityType.WallClimb) spriteRenderer.color = wallClimbColor; // <-- THÊM MỚI
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
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Hazard"))
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                // Yêu cầu GameManager bắt đầu chuỗi hành động hồi sinh
                gameManager.StartRespawn(this.gameObject);
            }
        }
    }
}