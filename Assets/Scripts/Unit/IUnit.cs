using System;
using UnityEngine;

public interface IUnit
{
    UnitStats Stats { get; }

    bool CanAttack(Unit unit);
    void Attack(Unit targetUnit);
    void TakeDamage(int damage);
    bool IsAlive();
    Action Die(Unit unit);

    Unit GetUnitType();
}

