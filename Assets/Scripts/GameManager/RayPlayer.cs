using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RayPlayer : MonoBehaviour
{
    [SerializeField] private RayHandler _rayHandler;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                if (hit.collider.TryGetComponent(out Unit unit))
                {
                    _rayHandler.HandleUnitClick(unit);
                }
                else if (hit.collider.TryGetComponent(out Cell cell))
                {
                    _rayHandler.HandleCellClick(cell);
                }
            }
        }
    }
}

public enum UnitActionType
{
    Move
}
