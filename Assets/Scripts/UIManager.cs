using UnityEngine;
using UnityEngine.UI; 

public class UIManager : MonoBehaviour {
    public Image[] hearts; // Một mảng để chứa các hình ảnh trái tim

    public void UpdateHealth(int currentHealth) {
        // Duyệt qua tất cả các trái tim
        for (int i = 0; i < hearts.Length; i++)
        {
            // Nếu chỉ số của trái tim (i) nhỏ hơn máu hiện tại -> hiển thị nó
            if (i < currentHealth)
            {
                hearts[i].enabled = true;
            }
            // Ngược lại -> ẩn nó đi
            else
            {
                hearts[i].enabled = false;
            }
        }
    }
}