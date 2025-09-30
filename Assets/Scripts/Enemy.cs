using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour {
    public int maxHealth = 3;
    private int currentHealth;
    public Slider healthSlider;

    void Start() {
        currentHealth = maxHealth;
        // Đặt giá trị tối đa cho slider
        healthSlider.maxValue = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage) {
        currentHealth -= damage;
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar() {
        if (healthSlider != null)
        {
            // Chỉ cần thay đổi giá trị của slider, nó sẽ tự co giãn
            healthSlider.value = currentHealth;
        }
    }

    void Die() {
        Debug.Log(gameObject.name + " đã bị tiêu diệt!");
        Destroy(gameObject);
    }
}