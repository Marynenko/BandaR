using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileState
{
    Standard, // ����������� ���������
    Selected, // ������ �������������
    Movement, // ������������ ������ ���� ���� ��� �������� �����
    OccupiedByPlayer, // ����� �������
    OccupiedByEnemy, // ����� ������
}

public static class TileColors
{
    public static Color TileColorStandard = Color.HSVToRGB(120, 1, 48);
    public static Color ColorPlayerOnTile = Color.HSVToRGB(111, 100, 53);
    public static Color ColorEnemyOnTile = Color.HSVToRGB(0, 100, 74);
    public static Color ColorSelectedTile = Color.HSVToRGB(0, 0, 81);
    public static Color ColorMovementTile = Color.HSVToRGB(97, 100, 100);
}
