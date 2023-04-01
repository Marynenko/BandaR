using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridInitializer : MonoBehaviour
{
    [SerializeField] private List<Unit> AllExistedUnits;

    public Grid Grid;

    public void InitializationGrid()
    {
        Grid.CreateGrid();
        Grid.SetNeighborsToAllCells();
        Grid.SetWalkableCells();
        Grid.SetImpassableCells();
        Grid.SetReachableCells();
        Grid.AddUnitsToCells(AllExistedUnits); // передаем список AllExistedUnits вместо использования Grid.AllUnits
    }
}


