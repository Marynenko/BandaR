using System.Collections.Generic;
using UnityEngine;

public class Player : Unit
{
    public override IUnit GetUnitType() => this;

    public new UnitStats Stats { get; private set; }

    private void Awake()
    {
        Stats = GetComponent<UnitStats>();
        // здесь можно добавить код инициализации других свойств юнита
    }

    // здесь можно добавить другие методы и свойства юнита
}
