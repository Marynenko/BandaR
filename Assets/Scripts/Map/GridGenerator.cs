using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GameController _gameController;
    [SerializeField] private GameModel _gameModel;
    [SerializeField] private List<Unit> _allExistedUnits;

    public Grid Grid;

    private void Start()
    {
        Grid.CreateGrid();
        Grid.LocateNeighborsTiles();

        _allExistedUnits ??= new List<Unit>(FindObjectsOfType<Unit>());
        
        Grid.AddUnitsToTiles(_allExistedUnits); // передаем список AllExistedUnits вместо использования Grid.AllUnits

        _gameModel.StartGame();
    }

}

