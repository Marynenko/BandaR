using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : Unit
{

    public override void TrackAllEnemies()
    {
        Enemies = new List<Unit>();
        // Enemies.Clear();
        var units = UIManager.Instance.TurnManager.Players;
        foreach (var unit in units.Where(unit => unit.Stats.Type is UnitType.Enemy))
        {
            Enemies.Add(unit);
        }
    }

    // здесь можно добавить другие методы и свойства юнита
}
