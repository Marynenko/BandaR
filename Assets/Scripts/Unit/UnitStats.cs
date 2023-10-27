using UnityEngine;

public class UnitStats : MonoBehaviour
{
    public int ID;
    public string Name;
    public UnitType Type;
    public float Health;
    public float Energy;
    public float EnergyForMove;
    public int CountAttacks;
    public int MaxCountAttacks;
    public float StateFatigue;
    public int MovementPoints;
    public int MovementRange;
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

