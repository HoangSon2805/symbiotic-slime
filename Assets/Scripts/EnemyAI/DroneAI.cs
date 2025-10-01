using UnityEngine;

public class DroneAI : MonoBehaviour {
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;

    private Transform targetTransform; // Dùng Transform thay vì Vector3

    void Start() {
        // Đặt vị trí ban đầu của Drone ngay tại PointA
        transform.position = pointA.position;
        // Bắt đầu bằng việc bay đến điểm B
        targetTransform = pointB;
    }

    void Update() {
        // Di chuyển đến vị trí mục tiêu
        transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, speed * Time.deltaTime);

        // Kiểm tra xem đã đến gần mục tiêu chưa
        if (Vector3.Distance(transform.position, targetTransform.position) < 0.1f)
        {
            // Nếu mục tiêu hiện tại là điểm B, đổi mục tiêu thành điểm A.
            // Ngược lại, đổi mục tiêu thành điểm B.
            targetTransform = (targetTransform == pointB) ? pointA : pointB;
        }
    }
}