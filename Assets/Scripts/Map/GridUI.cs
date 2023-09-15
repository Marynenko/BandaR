using System.Collections.Generic;
using System.Linq;

public static class GridUI
{
    public static void HighlightTile(Tile tile, TileState state) =>
        tile.ChangeColor(state);
    
    public static void HighlightTiles(IEnumerable<Tile> tiles, TileState state)
    {
        foreach (var tile in tiles)
            HighlightTile(tile, state);
    }
    
    public static void UnhighlightTile(Tile tile) => tile.Available = true;
    //
    // // TODO ����� ������ ������ ������� unit �� ����
    // public void HighlightAvailableMoves(Tile tile, List<Tile> availableMoves) // TODO ���������� ����� tile � ������ 1 ��� ���������?
    // {
    //     HighlightTile(SelectedUnit.OccupiedTile, tile.State);
    //
    //     var availableMovesCopy = availableMoves.GetRange(0, _availableMoves.Count);
    //     availableMovesCopy.Remove(tile);
    //     
    //     HighlightTiles(availableMovesCopy, TileState.Movement);
    // }
    
    public static void HighlightAvailableMoves(IReadOnlyList<Tile> availableMoves, TileState unitState)
    {
        HighlightTile(availableMoves.First(), unitState); // TODO ���������� ���� �� ���, ��� ��� ��� ���������� HighlightTile
        HighlightTiles(availableMoves.Skip(1), TileState.Movement);
    }
    
    public static void UnhighlightAvailableMoves(Tile currentTile)
    {
        HighlightTiles(currentTile.Neighbors, TileState.Movement);
    }
    
    public static void UnhighlightAllTiles(IReadOnlyList<Tile> Tiles)
    {
        HighlightTiles(Tiles, TileState.Standard);
    }
}
