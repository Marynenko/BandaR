using UnityEngine;

public enum TileState
{
    Standard, // Стандартное состояние
    Selected, // Выбран пользователем
    Movement, // Пользователь выбрал этот тайл для движения юнита
    OccupiedByPlayer, // Занят игроком
    OccupiedByEnemy, // Занят врагом
}

public class TileColors : MonoBehaviour
{
    //public Color TileColorStandard = new(45, 45, 45);
    //public Color ColorPlayerOnTile = new(20, 135, 0);
    //public Color ColorEnemyOnTile = new(189, 0, 0);
    //public Color ColorSelectedTile = new(204, 210, 203);
    //public Color ColorMovementTile = new(98, 225, 0);
    public Color TileColorStandard;
    public Color ColorPlayerOnTile;
    public Color ColorEnemyOnTile;
    public Color ColorSelectedTile;
    public Color ColorMovementTile;
}
