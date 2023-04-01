using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Enemy : Unit, IUnit
{
    public override IUnit GetUnitType() => this;
    //public override void Initialize(Cell cell)
    //{
    //    base.Initialize(cell);
    //    _playersInRange = new List<Unit>();
    //}

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
    //    foreach (Unit unit in _interactor.AllUnits)
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

