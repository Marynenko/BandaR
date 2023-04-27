using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GridInitializer _initializer;
    [SerializeField] private GameController _gameController;
    [SerializeField] private GameModel _gameModel;

    private void Start()
    {
        _initializer.InitializationGrid();
        _gameController.enabled = true;
    }
}

