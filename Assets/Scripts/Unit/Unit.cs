using DG.Tweening;
using System.Collections;
using UnityEngine;

public enum UnitType
{
    Player,
    Enemy
}

public enum UnitStatus
{
    Selected,
    Unselected,
    Moved
}

public class Unit : MonoBehaviour
{
    #region Variables
    private const float POSITION_Y = .8f;
    private const float MAX_DISTANCE = 3f;

    public delegate void UnitActionEventHandler(UnitActionType actionType, Unit unit, Cell cell);
    public static event UnitActionEventHandler OnUnitAction;

    public Cell CurrentCell;
    public UnitType Type;
    public UnitStatus Status = UnitStatus.Unselected;

    #endregion

    #region Public Methods
    public void Move(Cell targetCell)
    {
        if (CurrentCell == targetCell) return;
        if (Vector3.Distance(transform.position, targetCell.transform.position) > MAX_DISTANCE) return;

        if (Status != UnitStatus.Moved)
        {
            Status = UnitStatus.Moved;
            CurrentCell.RemoveUnit(this);
            targetCell.SetUnit(this);
            CurrentCell = targetCell;

            if (OnUnitAction != null)
            {
                OnUnitAction(UnitActionType.Move, this, targetCell);
            }

            MoveToCell(targetCell);
        }
    }

    public void MoveToCell(Cell cell)
    {
        Vector3 newPosition = new Vector3(cell.transform.position.x, POSITION_Y, cell.transform.position.z);
        transform.DOMove(newPosition, Vector3.Distance(transform.position, newPosition) / MAX_DISTANCE);
        CurrentCell.RemoveUnit(this);
        cell.SetUnit(this);
        CurrentCell = cell;
    }

    [ContextMenu("Initialize Unit")]
    public void InitializeUnit()
    {
        var ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, MAX_DISTANCE))
        {
            if (hit.collider.GetComponent<Cell>())
            {
                transform.position = new Vector3(hit.transform.position.x, POSITION_Y, hit.transform.position.z);
                CurrentCell = hit.collider.GetComponent<Cell>();
                CurrentCell.UnitOn = StatusUnitOn.Yes;
                Status = UnitStatus.Unselected;
            }

        }
    }
    #endregion

    #region trash

    //public IEnumerator MoveToCell(Cell cell)
    //{
    //    var targetPosition = cell.transform.position;

    //    while (transform.position != targetPosition)
    //    {
    //        transform.position = Vector3.MoveTowards(transform.position, targetPosition, MoveSpeed * Time.deltaTime);
    //        yield return null;
    //    }

    //    CurrentCell.RemoveUnit(this);
    //    CurrentCell = cell;
    //    //CurrentCell.SetUnit(this);
    //}
    #endregion
}
