using System.Collections.Generic;
using System.Linq;

public class Enemy : Unit
{
    public List<Unit> Enemies;

    protected override void TrackAllEnemies()
    {
        var units = GridUI.Instance.TurnManager.PlayersGet;
        foreach (var unit in units.Where(unit => unit.Stats.Type is UnitType.Player or UnitType.Ally))
            Enemies.Add(unit);
    }
    

    //protected override void Update()
    //{
    //    base.Update();
    //    if (IsPlayerInSight())
    //    {
    //        SelectTarget();
    //        if (_target != null)
    //        {
    //            Attack(_target);
    //        }
    //    }
    //}

    //private bool IsPlayerInSight()
    //{
    //    foreach (Unit unit in Selector.AllUnits)
    //    {
    //        if (unit.Type == UnitType.Player)
    //        {
    //            float distance = Vector3.Distance(transform.position, unit.transform.position);
    //            if (distance <= viewRange)
    //            {
    //                RaycastHit hit;
    //                if (Physics.Raycast(transform.position, unit.transform.position - transform.position, out hit))
    //                {
    //                    if (hit.transform == unit.transform)
    //                    {
    //                        _playersInRange.Add(unit);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    return _playersInRange.Count > 0;
    //}

    //private void SelectTarget()
    //{
    //    float minDistance = float.MaxValue;
    //    foreach (Unit unit in _playersInRange)
    //    {
    //        float distance = Vector3.Distance(transform.position, unit.transform.position);
    //        if (distance <= attackRange && distance < minDistance)
    //        {
    //            _target = unit;
    //            minDistance = distance;
    //        }
    //    }
    //}
}