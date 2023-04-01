using System;
using UnityEngine;

public enum UnitStatus
{
    Selected,
    Unselected,
    Moved
}

public enum UnitType
{
    Player,
    Enemy
}

public interface IUnit
{
    UnitStats Stats { get; }

    bool CanAttack(IUnit unit);
    void Attack(IUnit targetUnit);
    void TakeDamage(int damage);
    Action Die(IUnit unit);

    IUnit GetUnitType();
}

