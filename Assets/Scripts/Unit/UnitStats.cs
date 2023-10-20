using UnityEngine;

public class UnitStats : MonoBehaviour
{
    public int ID;
    public string Name;
    public UnitType Type;
    public float Health;
    public int Energy;
    public int EnergyForMove;
    public int EnergyForAttack;
    public int CountAttacks;
    public int StateFatigue;
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

