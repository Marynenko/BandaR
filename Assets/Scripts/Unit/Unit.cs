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
    Unselected,
    Moved
}

public class Unit : MonoBehaviour
{
    private const float _positionY = .8f;
    private const float _maxDistance = 3f;
    [SerializeField] private int _countMovementCell;

    [HideInInspector] public Vector2 Position;
    public UnitType Type;
    public UnitStatus Status;
    public Cell CurrentCell;
    
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
                CurrentCell.UnitOn = UnitOnStatus.Yes;
                Status = UnitStatus.Unselected;

            }

        }
    }

}
