using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridUI : MonoBehaviour
{
    
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

    public void HighlightTile(Tile tile, TileState state) =>
        tile.ChangeColor(state);
    
    public void HighlightTiles(IEnumerable<Tile> tiles, TileState state)
    {
        foreach (var tile in tiles)
            HighlightTile(tile, state);
    }

    public void HighlightAvailableMoves(HashSet<Tile> availableMoves, TileState unitState)
    {
        HighlightTile(availableMoves.First(), unitState); // TODO неизвестно нада ли это, так как уже вызывается HighlightTile
        HighlightTiles(availableMoves.Skip(1), TileState.Movement);
    }
    
    
}