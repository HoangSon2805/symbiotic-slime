using System.Collections;
using UnityEngine;

public class SlimeController : MonoBehaviour {
    // Public variables
    public float moveSpeed = 5f;
    public float jumpForce = 10f; // Đây là lực nhảy gốc
    public float highJumpMultiplier = 1.5f; // Hệ số nhân khi nhảy cao
    public Transform groundCheck;
    public LayerMask groundLayer;
    public Color highJumpColor = Color.yellow;

    // Biến cho hiệu ứng Squishy
    public float squashSpeed = 10f;

    // Private variables
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float horizontalInput;
    private bool isGrounded;
    private Vector3 originalScale;
    private bool wasGrounded;
    private Coroutine squashCoroutine;

    // Biến quản lý năng lực
    private bool hasHighJump = false; // Bây giờ biến này sẽ được SỬ DỤNG

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        // KHÔNG cần lưu originalJumpForce nữa vì ta sẽ không thay đổi jumpForce
    }

    void Update() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // <-- LOGIC SỬA ĐỔI NẰM Ở ĐÂY
            float currentJumpForce = jumpForce; // Bắt đầu với lực nhảy thường

            // Nếu có năng lực nhảy cao, thì nhân lực nhảy lên
            if (hasHighJump)
            {
                currentJumpForce *= highJumpMultiplier;
            }

            // Tác động lực đã được tính toán
            rb.velocity = new Vector2(rb.velocity.x, currentJumpForce);

            HandleSquashAndStretch(1.2f, 0.8f);
        }

        if (isGrounded && !wasGrounded)
        {
            HandleSquashAndStretch(0.8f, 1.2f);
        }

        wasGrounded = isGrounded;
    }

    void FixedUpdate() {
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.contacts[0].normal.y > 0.5f)
            {
                Absorb(collision.gameObject);
            } else
            {
                Debug.Log("Slime bị đụng trúng!");
            }
        }
    }

    void Absorb(GameObject enemy) {
        Debug.Log("Hấp thụ thành công!");
        Destroy(enemy);

        // <-- LOGIC SỬA ĐỔI NẰM Ở ĐÂY
        // Chỉ cần bật cờ hasHighJump lên là đủ
        hasHighJump = true;

        spriteRenderer.color = highJumpColor;
    }

    // Các hàm coroutine giữ nguyên, không thay đổi
    void HandleSquashAndStretch(float yScale, float xScale) {
        if (squashCoroutine != null)
        {
            StopCoroutine(squashCoroutine);
        }
        squashCoroutine = StartCoroutine(SquashAndStretchCoroutine(yScale, xScale));
    }

    IEnumerator SquashAndStretchCoroutine(float yScale, float xScale) {
        Vector3 targetScale = new Vector3(originalScale.x * xScale, originalScale.y * yScale, originalScale.z);
        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * squashSpeed);
            yield return null;
        }
        transform.localScale = targetScale;
        while (Vector3.Distance(transform.localScale, originalScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * squashSpeed);
            yield return null;
        }
        transform.localScale = originalScale;
    }
}