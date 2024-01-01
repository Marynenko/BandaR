using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridUI : MonoBehaviour
{
    public void HighlightAvailableMoves(HashSet<Tile> availableMoves, TileState unitState)
    {
        // Выделить первый тайл
        HighlightTile(availableMoves.First(), unitState);

        // Выделить остальные тайлы в зависимости от их состояния
        var standardTiles = availableMoves.Skip(1).Where(tile => tile.State == TileState.Standard);
        HighlightTiles(standardTiles, TileState.Movement);

        var occupiedByPlayerTiles = availableMoves.Skip(1).Where(tile => tile.State == TileState.OccupiedByPlayer);
        HighlightTiles(occupiedByPlayerTiles, TileState.OccupiedByPlayer);

        // var occupiedByAlliesTiles = availableMoves.Skip(1).Where(tile => tile.State == TileState.OccupiedByEnemy);
        // HighlightTiles(occupiedByAlliesTiles, TileState.OccupiedByAlly);

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

    public void ClearColorTiles(Tile[,] tiles)
    {
        foreach (var tile in tiles)
            if (!tile.IsAvailable())
                HighlightTile(tile, TileState.Standard);
    }
}