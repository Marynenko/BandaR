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
    Moved,
    Moving
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
    public int MaxMoves;

    #endregion

    #region Public Methods    

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

    public void MoveToCell(Cell targetCell)
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

            Vector3 newPosition = new Vector3(targetCell.transform.position.x, POSITION_Y, targetCell.transform.position.z);
            transform.DOMove(newPosition, Vector3.Distance(transform.position, newPosition) / MAX_DISTANCE);
        }
    }

    public bool CanAttack(Cell targetCell)
    {
        return false;
        // Проверка на возможность атаки
        // Например, проверка, что целевая клетка находится в пределах радиуса атаки и что на клетке находится юнит противника
        // Вернуть true если атака возможна, иначе false
    }

    public void Attack(Unit targetUnit)
    {
        // Реализация атаки на юнита
        // Например, вычисление урона, уменьшение здоровья целевого юнита, обработка смерти юнита, обновление интерфейса и т.д.
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
