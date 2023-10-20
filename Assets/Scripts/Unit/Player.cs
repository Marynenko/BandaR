using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : Unit
{
    public List<Unit> Enemies;
    public new UnitStats Stats { get; private set; }

    private void Awake()
    {
        Stats = GetComponent<UnitStats>();
        // здесь можно добавить код инициализации других свойств юнита
    }

    public override void TrackAllEnemies()
    {
        var units = GridUI.Instance.TurnManager.PlayersGet;
        foreach (var unit in units.Where(unit => unit.Stats.Type is UnitType.Enemy))
        {
            Enemies.Add(unit);
        }
    }

    // здесь можно добавить другие методы и свойства юнита
}
