using UnityEngine;

public class Projectile : MonoBehaviour {
    public float speed = 10f;
    public float lifetime = 2f;
    public enum ProjectileOwner { Player, Enemy }
    public ProjectileOwner owner;
    public int damage = 1; // Sát thương của viên đạn

    void Start() {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // Nếu viên đạn là của Player
        if (owner == ProjectileOwner.Player)
        {
            // Nếu chạm vào Enemy -> Gây sát thương cho Enemy
            if (other.CompareTag("Enemy"))
            {
                // (Sau này sẽ thêm script cho Enemy để chúng có máu)
                // Tạm thời, chúng ta sẽ chỉ hủy kẻ thù
                Destroy(other.gameObject);
                Destroy(gameObject); // Hủy viên đạn
                return;
            }
            // Nếu chạm vào Player -> Bỏ qua (đạn của mình không tự làm mình bị thương)
            if (other.CompareTag("Player"))
            {
                return;
            }
        }
        // Nếu viên đạn là của Enemy
        else if (owner == ProjectileOwner.Enemy)
        {
            // Nếu chạm vào Enemy -> Bỏ qua (đạn của kẻ thù không làm tổn thương kẻ thù khác)
            if (other.CompareTag("Enemy"))
            {
                return;
            }
            // Nếu chạm vào Player -> Gây sát thương cho Player
            if (other.CompareTag("Player"))
            {
                SlimeController player = other.GetComponent<SlimeController>();
                if (player != null)
                {
                    player.TakeDamage(damage, transform);
                }
                Destroy(gameObject); // Hủy viên đạn
                return;
            }
        }

        // Trong mọi trường hợp khác (chạm đất, tường...)
        Destroy(gameObject);
    }
}