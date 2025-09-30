using UnityEngine;

public class ParallaxBackground : MonoBehaviour {
    private float length, startpos;
    private GameObject cam;
    public float parallaxEffectMultiplier;

    void Start() {
        // Lấy vị trí bắt đầu và chiều dài của sprite
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;

        // Tìm camera chính
        cam = Camera.main.gameObject;
    }

    void LateUpdate() {
        // Tính toán khoảng cách tương đối camera đã di chuyển
        float temp = (cam.transform.position.x * (1 - parallaxEffectMultiplier));

        // Tính toán khoảng cách di chuyển của background
        float dist = (cam.transform.position.x * parallaxEffectMultiplier);

        // Di chuyển background
        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        // === LOGIC LẶP LẠI VÔ HẠN ===
        // Nếu background đã trôi đi một khoảng lớn hơn chiều dài của nó
        if (temp > startpos + length)
        {
            // Dịch chuyển nó về phía trước một đoạn bằng đúng chiều dài của nó
            startpos += length;
        } else if (temp < startpos - length)
        {
            // Dịch chuyển nó về phía sau một đoạn bằng đúng chiều dài của nó
            startpos -= length;
        }
    }
}