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

    public float MoveSpeed { get; private set; }

    public int MaxMoves;
    public int CurrentMoves;
    #endregion

    public void Select()
    {
        // ...
        if (OnUnitSelected != null)
        {
            OnUnitSelected.Invoke(this, this.Type);
        }
    }

    public void Move(Cell targetCell)
    {
        // ...
        if (OnUnitAction != null)
        {
            OnUnitAction.Invoke(UnitActionType.Move, this, targetCell);
        }
    }

    public void SetCurrentCell(Cell cell)
    {
        CurrentCell = cell;
    }

    public IEnumerator MoveToCell(Cell cell)
    {
        var targetPosition = cell.transform.position;

        while (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, MoveSpeed * Time.deltaTime);
            yield return null;
        }

        CurrentCell.RemoveUnit(this);
        CurrentCell = cell;
        //CurrentCell.SetUnit(this);
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
                CurrentCell.UnitOn = UnitOnStatus.Yes;
                Status = UnitStatus.Unselected;
            }

        }
    }

}
