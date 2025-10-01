using UnityEngine;

public enum AbilityType { None, HighJump, Dash, WallClimb, Shoot, DoubleJump } 

public class AbilityGrant : MonoBehaviour {
    public AbilityType abilityToGrant;
}