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

    public static void HighlightAvailableMoves(IReadOnlyList<Tile> availableMoves, TileState unitState)
    {
        HighlightTile(availableMoves.First(), unitState); // TODO неизвестно нада ли это, так как уже вызывается HighlightTile
        HighlightTiles(availableMoves.Skip(1), TileState.Movement);
    }
}