using UnityEngine;

public class InputPlayer : MonoBehaviour, IInputHandler
{
    [SerializeField] private GameController _gameController;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleLeftClick(Input.mousePosition);
    }

    public void HandleLeftClick(Vector3 mousePosition)
    {
        var ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            if (hit.collider.TryGetComponent(out Unit unit))
            {
                _gameController.HandleUnitClick(unit);
            }
            else if (hit.collider.TryGetComponent(out Cell cell))
            {
                _gameController.HandleCellClick(cell);
            }
        }
    }
}

public enum UnitActionType
{
    Move
}
