using UnityEngine;

public class AudioManager : MonoBehaviour {
    // Singleton Pattern: Đảm bảo chỉ có duy nhất một AudioManager trong game
    // và chúng ta có thể gọi nó dễ dàng từ bất cứ đâu.
    public static AudioManager instance;

    // Component "loa"
    private AudioSource audioSource;

    // Kéo các file âm thanh vào đây trong Inspector
    public AudioClip jumpSound;
    public AudioClip dashSound;
    public AudioClip takeDamageSound;
    public AudioClip absorbSound;
    public AudioClip getKeySound;
    public AudioClip dieSound;
    public AudioClip playerShootSound;

    private void Awake() {
        // Thiết lập Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Giữ cho AudioManager không bị hủy khi qua màn mới
        } else
        {
            Destroy(gameObject);
        }
    }

    void Start() {
        // Lấy component "loa"
        audioSource = GetComponent<AudioSource>();
    }

    // Các hàm public để các script khác có thể gọi
    public void PlayJumpSound() {
        if (jumpSound != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }

    public void PlayDashSound() {
        if (dashSound != null)
        {
            audioSource.PlayOneShot(dashSound);
        }
    }

    public void PlayTakeDamageSound() {
        if (takeDamageSound != null)
        {
            audioSource.PlayOneShot(takeDamageSound);
        }
    }

    public void PlayAbsorbSound() {
        if (absorbSound != null)
        {
            audioSource.PlayOneShot(absorbSound);
        }
    }

    public void PlayGetKeySound() {
        if (getKeySound != null)
        {
            audioSource.PlayOneShot(getKeySound);
        }
    }
    public void PlayDieSound() {
        if (dieSound != null)
        {
            audioSource.PlayOneShot(dieSound);
        }
    }
    public void PlayPlayerShootSound() {
        if (playerShootSound != null)
        {
            audioSource.PlayOneShot(playerShootSound);
        }
    }
}