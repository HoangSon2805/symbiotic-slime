using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : MonoBehaviour {
    // === Các biến Public (để chỉnh trong Inspector) ===
    [Header("Movement Stats")]
    public float moveSpeed = 5f;
    public float jumpForce = 5.5f;

    [Header("Dash Ability")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;

    [Header("Wall Ability")]
    public Transform wallCheck;
    public float wallSlidingSpeed = 2f;
    public Vector2 wallJumpForce = new Vector2(8f, 12f);

    [Header("Shoot Ability")] 
    public GameObject playerProjectilePrefab; // Prefab viên đạn của Slime
    public Transform playerFirePoint;         // Vị trí Slime bắn đạn ra
    public float playerFireRate = 0.5f;       // Tốc độ bắn của Slime

    [Header("Combat Stats")]
    public int maxHealth = 3;
    public Vector2 knockbackForce = new Vector2(5f, 5f);
    public float invincibilityDuration = 1f;
    public float highJumpMultiplier = 1.5f;

    [Header("Checks & Layers")]
    public Transform groundCheck;
    public LayerMask groundLayer;

    [Header("Effects")]
    public ParticleSystem jumpEffect;
    public ParticleSystem dashEffectRight;
    public ParticleSystem dashEffectLeft;
    public ParticleSystem damageParticles;

    [Header("UI Colors")]
    public Color highJumpColor = Color.yellow;
    public Color dashColor = Color.cyan;
    public Color wallClimbColor = Color.green;
    public Color doubleJumpColor = Color.magenta;

    // === Các biến Private (trạng thái và tham chiếu) ===
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private UIManager uiManager;
    private GameManager gameManager;
    private float horizontalInput;
    private int currentHealth;
    private float nextFireTime; // Thời gian có thể bắn tiếp
    private bool canDoubleJump = false;

    // === HỆ THỐNG STATE MACHINE ===
    public enum PlayerState { Idle, Running, Jumping, Falling, WallSliding, Dashing, Knockback, Dead }
    private PlayerState currentState;

    // Các biến kiểm tra trạng thái
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isFacingRight = true;
    private bool isInvincible = false;
    private bool canAirDash;

    // Hệ thống năng lực
    private List<AbilityType> unlockedAbilities = new List<AbilityType>();

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiManager = FindObjectOfType<UIManager>();
        gameManager = FindObjectOfType<GameManager>();
        HealToFull();
    }

    void Update() {
        if (currentState == PlayerState.Dead) return;

        // 1. LUÔN LUÔN KIỂM TRA MÔI TRƯỜNG & INPUT
        horizontalInput = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, 0.2f, groundLayer);

        // 2. XỬ LÝ QUAY MẶT (FLIP)
        if (currentState != PlayerState.WallSliding && currentState != PlayerState.Dashing && currentState != PlayerState.Knockback)
        {
            if (horizontalInput < 0 && isFacingRight) Flip();
            else if (horizontalInput > 0 && !isFacingRight) Flip();
        }

        // 3. BỘ NÃO STATE MACHINE
        UpdateState();

        // 4. XỬ LÝ CÁC HÀNH ĐỘNG TỨC THỜI (INPUT)
        HandleInput();
    }

    private void UpdateState() {
        if (currentState == PlayerState.Dashing || currentState == PlayerState.Knockback || currentState == PlayerState.Dead) return;

        if (isTouchingWall && !isGrounded && rb.velocity.y < 0 && HasAbility(AbilityType.WallClimb) && horizontalInput != 0)
        {
            ChangeState(PlayerState.WallSliding);
        } else if (rb.velocity.y < -0.1f && !isGrounded)
        {
            ChangeState(PlayerState.Falling);
        } else if (rb.velocity.y > 0.1f && !isGrounded)
        {
            ChangeState(PlayerState.Jumping);
        } else if (isGrounded && Mathf.Abs(horizontalInput) > 0.1f)
        {
            ChangeState(PlayerState.Running);
        } else if (isGrounded && Mathf.Abs(horizontalInput) < 0.1f)
        {
            ChangeState(PlayerState.Idle);
        }
    }

    private void FixedUpdate() {
        if (currentState == PlayerState.Dead || currentState == PlayerState.Dashing || currentState == PlayerState.Knockback) return;

        if (currentState == PlayerState.WallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        } else
        {
            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        }
    }

    private void HandleInput() {
        // Hồi lại cú lướt khi chạm đất hoặc tường
        if (isGrounded || currentState == PlayerState.WallSliding)
        {
            canAirDash = true;
            canDoubleJump = true;
        }

        // ===== LOGIC NHẢY ĐÃ ĐƯỢC CẢI TIẾN =====
        if (Input.GetButtonDown("Jump"))
        {
            // Ưu tiên 1: Nhảy Tường
            if (isTouchingWall && !isGrounded && HasAbility(AbilityType.WallClimb))
            {
                WallJump();
                // Khi nhảy tường, cũng nên reset lại canDoubleJump để có thể nhảy đôi sau đó
                canDoubleJump = true;
            }
            // Ưu tiên 2: Nhảy Thường (dưới đất)
            else if (isGrounded)
            {
                Jump();
            }
            // ƯU TIÊN 3: NHẢY ĐÔI (trên không)
            else if (HasAbility(AbilityType.DoubleJump) && canDoubleJump)
            {
                Jump(); // Dùng lại hàm Jump() thường
                canDoubleJump = false; // Dùng mất lượt nhảy đôi
            }
        }

        // Lướt (logic giữ nguyên)
        if (Input.GetKeyDown(KeyCode.LeftShift) && HasAbility(AbilityType.Dash) && canAirDash && currentState != PlayerState.Dashing)
        {
            StartCoroutine(DashCoroutine());
        }
        if (Input.GetButtonDown("Fire1") && HasAbility(AbilityType.Shoot) && Time.time >= nextFireTime)
        {
            // Bắn đạn
            Shoot();
            // Đặt lại thời gian có thể bắn tiếp
            nextFireTime = Time.time + playerFireRate;
        }
    }

    private void ChangeState(PlayerState newState) {
        if (newState == currentState) return;
        currentState = newState;
    }

    // === CÁC HÀM HÀNH ĐỘNG ===
    private void Jump() {
        float currentJumpForce = jumpForce * (HasAbility(AbilityType.HighJump) ? highJumpMultiplier : 1f);
        rb.velocity = new Vector2(rb.velocity.x, currentJumpForce);
        if (jumpEffect != null) jumpEffect.Play();
        AudioManager.instance.PlayJumpSound();
    }

    private void WallJump() {
        rb.velocity = new Vector2(wallJumpForce.x * (isFacingRight ? -1 : 1), wallJumpForce.y);
        Flip();
        if (jumpEffect != null) jumpEffect.Play();
        AudioManager.instance.PlayJumpSound();
    }
    private void Shoot() {
        if (playerProjectilePrefab == null || playerFirePoint == null) return;

        // Tạo ra viên đạn tại vị trí FirePoint của Slime
        GameObject newProjectile = Instantiate(playerProjectilePrefab, playerFirePoint.position, Quaternion.identity);

        // ===== GÁN PHE PHÁI CHO VIÊN ĐẠN =====
        newProjectile.GetComponent<Projectile>().owner = Projectile.ProjectileOwner.Player;

        // Điều chỉnh hướng của viên đạn dựa vào hướng mặt của Slime
        if (!isFacingRight)
        {
            // Nếu Slime quay trái, XOAY viên đạn lại 180 độ
            newProjectile.transform.Rotate(0f, 180f, 0f);
        }

        // Chơi âm thanh bắn
        AudioManager.instance.PlayPlayerShootSound();
    }
    private IEnumerator DashCoroutine() {
        ChangeState(PlayerState.Dashing);
        canAirDash = false;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2((isFacingRight ? 1 : -1) * dashForce, 0f);
        AudioManager.instance.PlayDashSound();

        // ===== LOGIC MỚI: CHỌN HIỆU ỨNG ĐÚNG HƯỚNG =====
        if (isFacingRight)
        {
            if (dashEffectRight != null) dashEffectRight.Play();
        } else
        {
            if (dashEffectLeft != null) dashEffectLeft.Play();
        }

        yield return new WaitForSeconds(dashDuration);

        // Dừng cả hai để chắc chắn
        if (dashEffectRight != null) dashEffectRight.Stop();
        if (dashEffectLeft != null) dashEffectLeft.Stop();

        rb.gravityScale = originalGravity;
        ChangeState(PlayerState.Falling);
    }

    public void TakeDamage(int damage, Transform damageSource) {
        if (isInvincible || currentState == PlayerState.Dead) return;

        // KÍCH HOẠT HIỆU ỨNG HẠT "MÁU" VĂNG RA
        if (damageParticles != null)
        {
            damageParticles.Play();
        }

        currentHealth -= damage;
        uiManager.UpdateHealth(currentHealth);
        AudioManager.instance.PlayTakeDamageSound();

        if (currentHealth <= 0)
        {
            ChangeState(PlayerState.Dead);
            gameManager.StartRespawn(this.gameObject);
        } else
        {
            StartCoroutine(InvincibilityCoroutine());
            StartCoroutine(KnockbackCoroutine(damageSource));
        }
    }

    private IEnumerator KnockbackCoroutine(Transform damageSource) {
        ChangeState(PlayerState.Knockback);
        Vector2 knockbackDirection = (transform.position - damageSource.position).normalized;
        rb.velocity = new Vector2(knockbackDirection.x * knockbackForce.x, knockbackDirection.y * knockbackForce.y);
        yield return new WaitForSeconds(0.3f);
        ChangeState(PlayerState.Falling);
    }

    private IEnumerator InvincibilityCoroutine() {
        isInvincible = true;
        for (float i = 0; i < invincibilityDuration; i += 0.1f)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f);
        }
        spriteRenderer.enabled = true;
        isInvincible = false;
    }

    private void Flip() {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    public void HealToFull() {
        currentHealth = maxHealth;
        isInvincible = false;
        ChangeState(PlayerState.Idle); // Reset trạng thái khi hồi sinh
        if (uiManager != null)
        {
            uiManager.UpdateHealth(currentHealth);
        }
    }

    private void Absorb(AbilityType ability, GameObject enemy) {
        Debug.Log("Hấp thụ năng lực: " + ability.ToString());
        Destroy(enemy);
        AudioManager.instance.PlayAbsorbSound();

        if (!HasAbility(ability))
        {
            unlockedAbilities.Add(ability);

            if (ability == AbilityType.HighJump) spriteRenderer.color = highJumpColor;
            if (ability == AbilityType.Dash) spriteRenderer.color = dashColor;
            if (ability == AbilityType.WallClimb) spriteRenderer.color = wallClimbColor;
            if (ability == AbilityType.DoubleJump) spriteRenderer.color = doubleJumpColor;
        }
    }

    private bool HasAbility(AbilityType ability) {
        return unlockedAbilities.Contains(ability);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // ƯU TIÊN KIỂM TRA CHẾT TỨC THÌ TRƯỚC
        if (other.CompareTag("InstaKillHazard"))
        {
            // Nếu là lửa -> Chết ngay, không cần qua TakeDamage
            if (currentState != PlayerState.Dead) // Kiểm tra để không gọi chết nhiều lần
            {
                ChangeState(PlayerState.Dead);
                gameManager.StartRespawn(this.gameObject);
            }
        }
        // Nếu không phải chết tức thì, mới kiểm tra các loại khác
        else if (other.CompareTag("Hazard"))
        {
            TakeDamage(1, other.transform);
        } else if (other.CompareTag("Key"))
        {
            gameManager.hasKey = true;
            AudioManager.instance.PlayGetKeySound();
            Destroy(other.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.contacts[0].normal.y > 0.5f)
            {
                AbilityGrant abilityGrant = collision.gameObject.GetComponent<AbilityGrant>();
                if (abilityGrant != null)
                {
                    Absorb(abilityGrant.abilityToGrant, collision.gameObject);
                }
            } else
            {
                TakeDamage(1, collision.transform);
            }
        } else if (collision.gameObject.CompareTag("Door"))
        {
            if (gameManager.hasKey)
            {
                Destroy(collision.gameObject);
            }
        }
    }

}