using System.Collections; 
using UnityEngine;
using Cinemachine;

public class GameManager : MonoBehaviour {
    public Transform respawnPoint;
    public float respawnDelay = 1f; // Thời gian chờ trước khi hồi sinh
    public CinemachineVirtualCamera virtualCamera;

    public bool hasKey = false;
    // Hàm này sẽ được gọi từ SlimeController
    public void StartRespawn(GameObject player) {
        // Bắt đầu một chuỗi hành động có độ trễ
        StartCoroutine(RespawnCoroutine(player));
    }

    // Đây là chuỗi hành động hồi sinh
    private IEnumerator RespawnCoroutine(GameObject player) {
        Debug.Log("Slime đã 'chết'!");

        // 1. Vô hiệu hóa người chơi

        // Tắt script điều khiển để người chơi không di chuyển được nữa
        player.GetComponent<SlimeController>().enabled = false;
        // Tắt hình ảnh để Slime biến mất
        player.GetComponent<SpriteRenderer>().enabled = false;
        // Tắt camera
        if (virtualCamera != null)
        {
            virtualCamera.Follow = null; // <-- RA LỆNH CHO CAMERA DỪNG THEO DÕI
        }

        // (Chúng ta sẽ thêm hiệu ứng 'nổ tung' ở đây sau này)

        // 2. Chờ
        // Dừng chuỗi hành động này lại trong 'respawnDelay' giây
        yield return new WaitForSeconds(respawnDelay);

        // 3. Hồi sinh người chơi
        Debug.Log("Đang hồi sinh!");
        // Di chuyển Slime về điểm hồi sinh
        player.transform.position = respawnPoint.position;
        // Rất quan trọng: Reset lại vận tốc của Slime về 0
        // Nếu không, Slime sẽ giữ nguyên vận tốc lúc chết và bay khỏi điểm hồi sinh
        player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        player.GetComponent<SlimeController>().HealToFull();
        // 4. Kích hoạt lại người chơi
        // Bật lại script điều khiển
        player.GetComponent<SlimeController>().enabled = true;
        // Bật lại hình ảnh
        player.GetComponent<SpriteRenderer>().enabled = true;
        if (virtualCamera != null)
        {
            virtualCamera.Follow = player.transform; // <-- RA LỆNH CHO CAMERA THEO DÕI LẠI
        }
    }
}