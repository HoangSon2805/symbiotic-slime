using System.Collections;
using UnityEngine;

public class DasherAI : MonoBehaviour {
    [Header("Dash Stats")]
    public float dashSpeed = 8f;     // Tốc độ khi lướt
    public float dashDuration = 0.3f; // Thời gian lướt
    public float timeBetweenDashes = 3f; // Thời gian chờ giữa mỗi lần lướt
    public float dashDistance = 3f; // Khoảng cách Dasher sẽ di chuyển khi dash (đứng tại chỗ, nhưng di chuyển một đoạn)

    private Rigidbody2D rb;
    private bool isFacingRight = true;
    //private bool isDashing = false;
    private Vector2 startPosition; // Vị trí ban đầu để Dasher luôn quay về

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position; // Ghi lại vị trí ban đầu
        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine() {
        // Vòng lặp vô tận cho hành vi dash
        while (true)
        {
            // Giai đoạn chờ
            yield return new WaitForSeconds(timeBetweenDashes);

            // Giai đoạn chuẩn bị Dash (có thể thêm animation/âm thanh ở đây)
            Flip(); // Quay đầu trước khi dash để dash theo hướng ngược lại

            // Bắt đầu Dash
            //isDashing = true;
            // Áp dụng lực dash một lần
            rb.velocity = new Vector2(isFacingRight ? dashSpeed : -dashSpeed, rb.velocity.y);

            // Chờ hết thời gian dash
            yield return new WaitForSeconds(dashDuration);

            // Kết thúc Dash
            //isDashing = false;
            rb.velocity = Vector2.zero; // Ngừng di chuyển sau dash
            transform.position = startPosition; // Đảm bảo Dasher quay về vị trí ban đầu

            // (Tùy chọn) Thêm một chút delay nữa nếu muốn Dasher dừng lại lâu hơn
            // yield return new WaitForSeconds(0.5f);
        }
    }

    // FixedUpdate không còn dùng để điều khiển di chuyển liên tục nữa
    void FixedUpdate() {
        // Có thể dùng để cập nhật animation hoặc các logic khác nếu cần
        // Hiện tại không cần dùng FixedUpdate nếu Dash chỉ dùng lực một lần
    }

    void Flip() {
        isFacingRight = !isFacingRight;
        // Xoay hình ảnh
        transform.Rotate(0f, 180f, 0f);
    }
}