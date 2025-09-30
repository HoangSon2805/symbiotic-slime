using UnityEngine;

public class BouncerAI : MonoBehaviour {
    public float jumpForce = 8f;
    public float jumpInterval = 2f; // Thời gian giữa mỗi lần nhảy

    private Rigidbody2D rb;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        // Gọi hàm Jump liên tục sau một khoảng thời gian
        InvokeRepeating("Jump", jumpInterval, jumpInterval);
    }

    void Jump() {
        // Kiểm tra xem nó có đang trên mặt đất không để tránh nhảy giữa không trung
        // Chúng ta sẽ dùng một cách đơn giản: nếu vận tốc dọc gần bằng 0 thì coi là đang trên đất
        if (Mathf.Abs(rb.velocity.y) < 0.05f)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }
}