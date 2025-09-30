using UnityEngine;

public class SpitterAI : MonoBehaviour {
    [Header("References")]
    public GameObject projectilePrefab; // Prefab của viên đạn
    public Transform firePoint;         // Vị trí nòng súng

    [Header("Stats")]
    public float fireRate = 2f;         // Cứ 2 giây bắn 1 lần

    private Transform player;           // Tham chiếu đến người chơi

    void Start() {
        // Tự động tìm người chơi khi game bắt đầu
        player = FindObjectOfType<SlimeController>().transform;

        // Lặp lại việc gọi hàm "Shoot" sau 2 giây, và cứ 2 giây gọi lại 1 lần
        InvokeRepeating("Shoot", fireRate, fireRate);
    }

    void Shoot() {
        if (player == null) return; // Nếu không tìm thấy người chơi thì không làm gì cả

        // 1. Tính toán hướng bắn
        // Vector hướng đi từ vị trí của nòng súng đến vị trí người chơi
        Vector2 direction = (player.position - firePoint.position).normalized;

        // 2. Tính toán góc quay
        // Dùng hàm Atan2 để tính góc (radian), sau đó đổi sang độ (degree)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 3. Tạo ra viên đạn
        // Quaternion.Euler(0, 0, angle) là cách tạo ra một góc quay trong không gian 2D
        Instantiate(projectilePrefab, firePoint.position, Quaternion.Euler(0, 0, angle));
    }
}