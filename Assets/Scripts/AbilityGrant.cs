using UnityEngine;

public enum AbilityType { None, HighJump, Dash, WallClimb } 

public class AbilityGrant : MonoBehaviour {
    public AbilityType abilityToGrant;
}