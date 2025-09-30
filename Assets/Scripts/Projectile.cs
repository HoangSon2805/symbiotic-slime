using UnityEngine;

public class Projectile : MonoBehaviour {
    public float speed = 10f;
    public float lifetime = 2f; // Đạn sẽ tự hủy sau 2 giây

    void Start() {
        // Lấy component Rigidbody2D
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        // Bắn viên đạn đi theo hướng "phía trước" của nó (trục X màu xanh dương)
        rb.velocity = transform.right * speed;

        // Hẹn giờ tự hủy
        Destroy(gameObject, lifetime);
    }

    // Xử lý khi đạn va chạm
    private void OnTriggerEnter2D(Collider2D other) {
        // NẾU VA CHẠM VỚI VẬT THỂ CÓ TAG "ENEMY" -> BỎ QUA, KHÔNG LÀM GÌ CẢ
        if (other.CompareTag("Enemy"))
        {
            return; // Dừng hàm tại đây, cho phép viên đạn bay xuyên qua
        }

        // Nếu va chạm với bất cứ thứ gì khác (ví dụ: Player, Ground...)
        // thì mới tự hủy
        Destroy(gameObject);
    }
}