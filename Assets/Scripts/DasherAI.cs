using UnityEngine;

public class DasherAI : MonoBehaviour {
    public float moveSpeed = 3f;
    public Transform wallCheck; // Điểm để kiểm tra tường
    public LayerMask groundLayer; // Lấy luôn groundLayer để biết đâu là tường
    private Rigidbody2D rb;
    private bool isFacingRight = true;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate() {
        // Di chuyển
        rb.velocity = new Vector2(isFacingRight ? moveSpeed : -moveSpeed, rb.velocity.y);

        // Kiểm tra xem có chạm tường không
        bool isHittingWall = Physics2D.OverlapCircle(wallCheck.position, 0.1f, groundLayer);

        if (isHittingWall)
        {
            Flip();
        }
    }

    // Hàm để quay đầu
    void Flip() {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f); // Lật sprite lại
    }
}