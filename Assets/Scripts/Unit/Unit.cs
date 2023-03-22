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

    [SerializeField] private UnitType _unitType;

    public delegate void UnitSelectedEventHandler(Unit unit, UnitType unitType);
    public static event UnitSelectedEventHandler OnUnitSelected;

    public delegate void UnitActionEventHandler(UnitActionType actionType, Unit unit, Cell cell);
    public static event UnitActionEventHandler OnUnitAction;

    public Cell CurrentCell;
    public UnitType Type;
    public UnitStatus Status = UnitStatus.Unselected;

    #endregion

    #region Public Methods

    public void Select()
    {
        if (Status == UnitStatus.Selected && OnUnitSelected != null)
        {
            return;
        }

        Status = UnitStatus.Selected;
        OnUnitSelected?.Invoke(this, Type);
    }

    public void Move(Cell targetCell)
    {
        if (Status == UnitStatus.Moved && OnUnitAction != null)
        {
            return;
        }

        // Проверяем может ли юнит переместиться на целевую ячейку
        if (targetCell != CurrentCell && Vector3.Distance(targetCell.transform.position, transform.position) <= MAX_DISTANCE)
        {
            CurrentCell.RemoveUnit(this);
            targetCell.SetUnit(this);
            transform.position = new Vector3(targetCell.transform.position.x, POSITION_Y, targetCell.transform.position.z);
            OnUnitAction?.Invoke(UnitActionType.Move, this, targetCell);
        }
    }

    public void MoveToCell(Cell cell)
    {
        if (Status == UnitStatus.Moved)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, cell.transform.position);

        if (distance > MAX_DISTANCE)
        {
            return;
        }

        transform.position = new Vector3(cell.transform.position.x, POSITION_Y, cell.transform.position.z);
        Move(cell);
    }

    //public void MoveToCell(Cell cell)
    //{
    //    if (cell != null && Vector3.Distance(cell.transform.position, transform.position) <= MAX_DISTANCE)
    //    {
    //        CurrentCell.RemoveUnit(this);
    //        cell.SetUnit(this);
    //        transform.position = new Vector3(cell.transform.position.x, POSITION_Y, cell.transform.position.z);
    //        OnUnitAction?.Invoke(UnitActionType.Move, this, cell);
    //    }
    //}
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
                CurrentCell.UnitOn = UnitOnStatus.Yes;
                Status = UnitStatus.Unselected;
            }

        }
    }
    #endregion
}
