using System;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType
{
    Player,
    Enemy
}

public enum UnitStatus
{
    Selected,
    Unselected
}

public class Unit : MonoBehaviour
{
    private const float _positionY = .8f;
    private const float _maxDistance = 3f;
    [SerializeField] private int _countMovementCell;
    [SerializeField] private List<Vector2> _possibleMovements;
    
    public UnitType Type;
    public UnitStatus Status;

    public Cell CurrentCell;
    [HideInInspector] public Vector2 Position;

    public bool CheckUnitOnCell(Cell cellToCheck)
    {
        var ray = ReturnCurrentPosition();
        RaycastHit unitPositionHitCell;

        if (Physics.Raycast(ray, out unitPositionHitCell, _maxDistance))
        {
            if (unitPositionHitCell.collider.GetComponent<Cell>().Position.Equals(cellToCheck.Position))
            {
                CurrentCell = unitPositionHitCell.collider.GetComponent<Cell>();
                CurrentCell.EUnitOn = UnitOn.Yes;
                Status = UnitStatus.Selected;

                return true;
            }
        }
        else
        {
            //Status = UnitStatus.Unselected;
            CurrentCell.EUnitOn = UnitOn.No;
        }

        return false;
    }

    private Ray ReturnCurrentPosition() => new Ray(transform.position, Vector3.down);

    private void ShowPossibleMovement()
    {
        CalculatePossibleMovements(CurrentCell, _countMovementCell);
    }

    private void CalculatePossibleMovements(Cell currentCell, int countMovementCell)
    {
        //Vector2 possibleMovement = currentCell.Position + .05f;
    }

    
    [ContextMenu("Initialize Unit")]
    public void InitializeUnit()
    {
        var ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _maxDistance))
        {
            if (hit.collider.GetComponent<Cell>())
            {
                transform.position = new Vector3(hit.transform.position.x, _positionY, hit.transform.position.z);

                CurrentCell = hit.collider.GetComponent<Cell>();
                CurrentCell.EUnitOn = UnitOn.Yes;
                Status = UnitStatus.Unselected;
            }

        }
    }

}
