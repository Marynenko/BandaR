using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ally : Unit
{
    public List<Unit> Enemies;
    
    protected override void TrackAllEnemies()
    {
        
        var units = GridUI.Instance.TurnManager.PlayersGet;
        foreach (var unit in units.Where(unit => unit.Stats.Type is UnitType.Enemy))
        {
            Enemies.Add(unit);
        }
    }
}