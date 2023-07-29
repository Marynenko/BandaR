﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Transform Parent;
    public Cell CellPrefab; 
    public Vector2Int GridSize;
    public float Offset;

    public GridInteractor Interactor { get; private set; }
    public GridInitializer Initializer { get; private set; }
    public List<Unit> AllUnits { get; private set; }
    public Cell[,] Cells { get; private set; }

    private void Awake()
    {
        Cells = new Cell[GridSize.x, GridSize.y];
        AllUnits = new List<Unit>();
        Initializer = GetComponent<GridInitializer>();
        Interactor = GetComponentInChildren<GridInteractor>();
        Interactor.enabled = true; // Включаем интерактор после инициализации сетки

    }
    public void CreateGrid()
    {
        var cellSize = CellPrefab.GetComponent<MeshRenderer>().bounds.size;

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
            {
                // Чтобы сгенерировать клетку, нужно знать ее позицию.
                var position = new Vector3(x * (cellSize.x + Offset), 0, y * (cellSize.z + Offset));

                var cell = Instantiate(CellPrefab, position, Quaternion.identity, Parent);
                cell.Initialize(x, y, Interactor, true, false); // тут передается Grid

                Cells[x, y] = cell;
            }
    }

    public void LocateNeighboursCells()
    {
        foreach (var cell in Cells)
            cell.Neighbours = Interactor.PathConstructor.GetNeighbourCells(cell, this); // Добавли левую часть.
    }

    public void SetAvaialableCells()
    {
        foreach (var cell in Cells)
            if (!cell.IsOccupied())
                cell.SetAvailable(true);
    }

    public void AddUnitsToCells(List<Unit> units)
    {       
        foreach (var unit in units)
        {            
            Vector2Int unitCellCoordinates = GetCellCoordinatesFromPosition(unit.transform.position);
            Cell cell = Cells[unitCellCoordinates.x, unitCellCoordinates.y];

            if (unitCellCoordinates != Vector2Int.one * int.MaxValue)
            {
                unit.InitializeUnit(this, cell);
                AllUnits.Add(unit);
            }
        }
    }

    public void RemoveUnit(Unit unit)
    {
        var unitToRemove = unit as Unit;
        Cell currentCell = unitToRemove.CurrentCell;

        if (currentCell != null)
        {
            //currentCell.ClearUnit();
            AllUnits.Remove(unitToRemove);
            Destroy(unitToRemove.gameObject);
        }
    }

    public Vector2Int GetCellCoordinatesFromPosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / CellPrefab.GetCellSize().x);
        int y = Mathf.FloorToInt(position.z / CellPrefab.GetCellSize().z);

        return new Vector2Int(x, y);
    }

}
