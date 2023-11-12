using UnityEngine;

public class UnitStats : MonoBehaviour
{
    [Header("Unit identity")]
    public int ID;
    public string Name;
    public UnitType Type;
    public int MaxCountAttacks;
    public int CountAttacks;
    public int MovementPoints;
    public int MovementRange;
    [Header("Characteristics")]
    public float MaxHealth;
    public float Health;
    public float Energy;
    public float EnergyForMove;
    public float StateFatigue;
    public int Speed;

}
public enum UnitType
{
    Player,
    Ally,
    Enemy
}

public enum UnitStatus
{
    Moved,
    Available,
    Unavailable,
    AIMove,
}

public enum UnitAttackSite
{
    Front,
    Back,
    Left,
    Right,
    None
}

