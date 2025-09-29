using UnityEngine;

public class SlimeController : MonoBehaviour {
    // Public variables: Có thể chỉnh các giá trị này trực tiếp trong Unity Editor
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public Transform groundCheck;
    public LayerMask groundLayer;

    // Private variables
    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;

    // Hàm này được gọi 1 lần khi game bắt đầu
    void Start() {
        // Lấy component Rigidbody2D từ chính object Slime
        rb = GetComponent<Rigidbody2D>();
    }

    // Hàm này được gọi mỗi frame
    void Update() {
        // 1. Lấy input từ người chơi (phím A, D hoặc mũi tên trái, phải)
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. Kiểm tra xem Slime có đang chạm đất không
        // Physics2D.OverlapCircle tạo một vòng tròn vô hình tại vị trí groundCheck
        // Nếu vòng tròn đó chạm vào bất cứ thứ gì thuộc groundLayer, isGrounded = true
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // 3. Xử lý nhảy: Nếu người chơi nhấn Space và Slime đang trên mặt đất
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Tác động một lực hướng lên trên để Slime nhảy
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    // Hàm này được gọi theo một chu kỳ vật lý cố định, tốt hơn cho việc xử lý vật lý
    void FixedUpdate() {
        // 4. Xử lý di chuyển
        // Thay đổi vận tốc của Rigidbody theo chiều ngang
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }
}