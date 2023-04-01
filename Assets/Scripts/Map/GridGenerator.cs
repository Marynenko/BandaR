using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GridInitializer Initializer;
    public GameController GameController;

    private void Start()
    {
        Initializer.InitializationGrid();
        GameController.enabled = true;
    }




}

