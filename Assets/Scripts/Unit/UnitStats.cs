using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStats : MonoBehaviour
{
    public int ID;
    public UnitType Type;
    public float ViewRange;
    public int MovementPoints;
    public int MovementRange;
    public int AttackDamage;
    public float AttackRange;
    public int Health;
}
public enum UnitType
{
    Player,
    Enemy
}

public enum UnitStatus
{
    //Selected,
    //Unselected,
    Moved,
    Available,
    Unavailable,
    Waiting,
    AIMove
}
