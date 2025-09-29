using UnityEngine;

// Đây là một Enum. Nó định nghĩa một danh sách các hằng số.
// Giúp code dễ đọc và tránh lỗi gõ sai chữ.
public enum AbilityType { None, HighJump, Dash }

public class AbilityGrant : MonoBehaviour {
    public AbilityType abilityToGrant;
}