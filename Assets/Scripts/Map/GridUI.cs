using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridUI : MonoBehaviour
{
    public TurnManager TurnManager;
    
    private static GridUI _instance;
    public static GridUI Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<GridUI>();
            return _instance;
        }
    }

    private void OnEnable()
    {
        Instance.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        Instance.gameObject.SetActive(false);
    }

    public void HighlightAvailableMoves(HashSet<Tile> availableMoves, TileState unitState)
    {
        // �������� ������ ����
        HighlightTile(availableMoves.First(), unitState);

        // �������� ��������� ����� � ����������� �� �� ���������
        var standardTiles = availableMoves.Skip(1).Where(tile => tile.State == TileState.Standard);
        HighlightTiles(standardTiles, TileState.Movement);

        var occupiedByPlayerTiles = availableMoves.Skip(1).Where(tile => tile.State == TileState.OccupiedByPlayer);
        HighlightTiles(occupiedByPlayerTiles, TileState.OccupiedByPlayer);

        var occupiedByEnemyTiles = availableMoves.Skip(1).Where(tile => tile.State == TileState.OccupiedByEnemy);
        HighlightTiles(occupiedByEnemyTiles, TileState.OccupiedByEnemy);
    }
 
    public void HighlightTiles(IEnumerable<Tile> tiles, TileState state)
    {
        foreach (var tile in tiles)
            HighlightTile(tile, state);
    }
    
    public void HighlightTile(Tile tile, TileState state) =>
        tile.ChangeColor(state);
}