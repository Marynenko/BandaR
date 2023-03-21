using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public enum State
{
    Standard,
    Selected,
    Movement,
    Impassable,
    Reachable,
    Default
}

public enum UnitOnStatus // test v.1
{
    Yes,
    No
}

public class Cell : MonoBehaviour
{
    public GridInteractor GICell;
    [SerializeField] private MeshRenderer MeshRenderer;

    [HideInInspector] public State EUnitState; // Состояние клетки.
    [HideInInspector] public UnitOnStatus UnitOn; // Юнит на клетке или нет.    
    [HideInInspector] public Vector2 Position; // Позиция Клетки.


    public Color CellStandardColor; //Стандартный цвет клетки.
    public Color CellHoveringColor;// Цвет при наведении на клетку.
    public Color CellUnitOnColor; // Цвет клетки на которой стоит гл. герой.
    public Color CellEnemyOnColor; // Цвет клетки на которой стоит враг.
    public Color CellSelectedColor; // Цвет клетки - выбранной
    public Color CellMovementColor; // Цвет клетки - Для движения


    // Юнит который на клетке. ДОПИЛИТЬ

    public void ChangeColor(Color color)
    {
        MeshRenderer.material.color = color;
    }

}
