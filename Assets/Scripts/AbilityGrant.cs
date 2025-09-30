using UnityEngine;

public enum AbilityType { None, HighJump, Dash, WallClimb, Shoot }

public class AbilityGrant : MonoBehaviour {
    public AbilityType abilityToGrant;
}