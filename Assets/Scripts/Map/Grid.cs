using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{   
    [SerializeField] protected GridInteractor GGridInteractor;
    public List<Unit> Units;

    private void Start()
    {
        


    }

    protected void Initialization()
    {
        foreach (var cell in GGridInteractor.Cells)
        {
            int x = Convert.ToInt32(cell.Coordinates.x);
            int y = Convert.ToInt32(cell.Coordinates.y);
            cell.Initialize(x, y, GGridInteractor, cell.IsWalkable(), cell.UnitOn);
        }
        // Получить все Unit в игре и добавить их в список Units
        //Units = FindObjectsOfType<Unit>().ToList();
        foreach (Unit unit in Units)
        {
            unit.InitializeUnit();
        }
    }

}
