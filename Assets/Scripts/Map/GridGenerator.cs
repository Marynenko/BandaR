using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GameController _gameController;
    [SerializeField] private GameModel _gameModel;
    [SerializeField] private List<Unit> AllExistedUnits;

    public Grid Grid;

    private void Start()
    {
        Grid.CreateGrid();
        Grid.LocateNeighboursTiles();
        Grid.AddUnitsToTiles(AllExistedUnits); // передаем список AllExistedUnits вместо использования Grid.AllUnits

        _gameModel.StartGame();
    }

}

