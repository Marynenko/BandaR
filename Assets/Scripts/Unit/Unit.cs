using System.Collections;
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
    Unselected,
    Moved
}

public class Unit : MonoBehaviour
{
    private const float POSITION_Y = .8f;
    private const float MAX_DISTANCE = 3f;

    [SerializeField] private UnitType _unitType;

    [SerializeField] private int _maxHealth;
    [SerializeField] private int _currentHealth;
    [SerializeField] private int _damage;
    [SerializeField] private int _armor;

    public Cell CurrentCell;
    public UnitType Type => _unitType;
    public UnitStatus Status {get; set;}

    public int MaxHealth { get => _maxHealth; }
    public int CurrentHealth { get => _currentHealth; }
    public int Damage { get => _damage; }
    public int Armor { get => _armor; }
    public float MoveSpeed { get; private set; }

    public int MaxMoves;
    public int CurrentMoves;    

    public void SetCurrentCell(Cell cell)
    {
        CurrentCell = cell;
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Code to handle death of unit
        Destroy(gameObject);
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

    public void Attack(Unit otherUnit)
    {
        int finalDamage = Mathf.Max(_damage - otherUnit.Armor, 0);
        otherUnit.TakeDamage(finalDamage);
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
