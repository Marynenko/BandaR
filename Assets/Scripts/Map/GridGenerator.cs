using UnityEngine;

public class GridGenerator : Grid
{
    [SerializeField] private Transform _parent;
    [SerializeField] private Cell _cell;    
    [SerializeField] private float _offset;

    public Vector2Int GridSize;

    [ContextMenu("Generate grid")]
    private void GenerateGrid()
    {
        var cellSize = _cell.GetComponent<MeshRenderer>().bounds.size;

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                // Что бы сгенерировать клетку, нужно знаеть ее поз.
                var position = new Vector3(x * (cellSize.x + _offset), 0, y * (cellSize.z + _offset));

                var cell = Instantiate(_cell, position, Quaternion.identity, _parent);
                cell.Initialize(x, y, GGridInteractor, true, StatusUnitOn.No);

                GGridInteractor.Cells.Add(cell);
            }
        }
    }
}
