using System.Collections;
using UnityEngine;

public class SlimeController : MonoBehaviour {
    // Public variables
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public Transform groundCheck;
    public LayerMask groundLayer;

    // Biến cho hiệu ứng Squishy
    public float squashAmount = 0.8f;
    public float squashSpeed = 10f;

    // Private variables
    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    private Vector3 originalScale;
    private bool wasGrounded;

    private Coroutine squashCoroutine; // <-- THAY ĐỔI: Thêm một biến để lưu trữ coroutine đang chạy

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
    }

    void Update() {
        // 1. Lấy input
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. Kiểm tra chạm đất
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // 3. Xử lý nhảy
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            // Bắt đầu hiệu ứng giãn ra khi nhảy
            HandleSquashAndStretch(1.2f, 0.8f); // <-- THAY ĐỔI: Gọi qua hàm quản lý
        }

        // 4. Kiểm tra thời điểm vừa tiếp đất
        if (isGrounded && !wasGrounded)
        {
            // Bắt đầu hiệu ứng co lại khi tiếp đất
            HandleSquashAndStretch(0.8f, 1.2f); // <-- THAY ĐỔI: Gọi qua hàm quản lý
        }

        wasGrounded = isGrounded;
    }

    void FixedUpdate() {
        // 5. Xử lý di chuyển
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }

    // <-- THAY ĐỔI: Tạo một hàm riêng để quản lý việc bắt đầu Coroutine
    void HandleSquashAndStretch(float yScale, float xScale) {
        // Nếu có một coroutine co giãn đang chạy, hãy dừng nó lại
        if (squashCoroutine != null)
        {
            StopCoroutine(squashCoroutine);
        }
        // Bắt đầu coroutine mới và lưu nó vào biến
        squashCoroutine = StartCoroutine(SquashAndStretchCoroutine(yScale, xScale));
    }

    // Đổi tên hàm để tránh nhầm lẫn
    IEnumerator SquashAndStretchCoroutine(float yScale, float xScale) {
        Vector3 targetScale = new Vector3(originalScale.x * xScale, originalScale.y * yScale, originalScale.z);

        // Co/giãn đến kích thước mục tiêu
        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * squashSpeed);
            yield return null;
        }
        transform.localScale = targetScale;

        // Trở về kích thước ban đầu
        while (Vector3.Distance(transform.localScale, originalScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * squashSpeed);
            yield return null;
        }
        transform.localScale = originalScale;
    }
}