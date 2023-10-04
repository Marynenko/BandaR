using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GridUI : MonoBehaviour
{
    private static GridUI _instance;
    public Color HighlightColor; // Цвет для выделения рамки

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

    public void HighlightAvailableMoves(HashSet<Tile> availableMoves, TileState unitState)
    {
        // Выделить первый тайл
        HighlightTile(availableMoves.First(), unitState);

        // Выделить остальные тайлы в зависимости от их состояния
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
    
    private void HighlightTile(Tile tile, TileState state) =>
        tile.ChangeColor(state);
    
    public void HighlightCurrentPlayer(Image playerPortrait, bool isFinished = false)
    {
        var uiUnit = playerPortrait.GetComponentInParent<UIUnit>();

        if (isFinished)
            uiUnit.TurnOnAlpha();
        else
            uiUnit.TurnOffAlpha();
        // var parent = playerPortrait.GetComponentInParent<Image>();
        // var outline = parent.GetComponentInChildren<Outline>(); 

        // if (isFinished)
        //     outline.TurnOnAlpha();
        // else
        //     outline.TurnOffAlpha();

        // outline.SetColor(HighlightColor);
        // playerPortrait.GetComponent<Outline>().Get = HighlightColor;

        // // Найдите портрет текущего игрока и выделите его рамку
        // Image currentPortrait = GetPlayerPortrait(currentPlayer);
        // if (currentPortrait != null)
        // {
        //     currentPortrait.GetComponent<Outline>().effectColor = highlightColor;
        // }
    }
    
}