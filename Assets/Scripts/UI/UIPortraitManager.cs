using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPortraitManager : MonoBehaviour
{
    public GameObject UIPlayerPrefab; // ������ UIPlayer
    public GameObject UIEnemyPrefab; // ������ UIEnemy
    public GameObject UIAllyPrefab; // ������ UIPlayer
    public Dictionary<string, Image> UIPlayerPortraits = new();
    public Dictionary<Image, UIUnit> UIBackground = new();

    // ���������� ��� �������� �������� �� ����� �� ��������
    private void GetPortrait(string unitName, Image portraitImage)
    {
        // ���������� ��������� �������� �� ��������
        var portraitTexture = Resources.Load<Texture2D>("Portraits/" + unitName);
        if (portraitTexture == null) return;
        var rect = new Rect(0, 0, portraitTexture.width, portraitTexture.height);
        // ���������� �������� ��� ����������� ��� ���������� Image
        portraitImage.sprite = Sprite.Create(portraitTexture, rect, new Vector2(.5f, .5f));
    }

    private void UpdatePortraits(Unit unit)
    {
        // �������� ������� ������
        var playerPortrait = GetPlayerPortrait(unit);

        // ���� ������� �� ������, �� ������ �� ������
        if (playerPortrait == null) return;

        // �������� ��� ��������
        var playerBackground = GetPlayerBackground(playerPortrait);

        // ���� ��� �� ������, �� ������ �� ������
        if (playerBackground == null) return;

        // ��������� ������� � ��� � ������������ � ����� ���������� ������
        // ����� �� ������ �������� ���� ��� ��� ���������� �������� � ����
    }

    public void AddPortraits(List<Unit> units)
    {
        foreach (var unit in units)
        {
            var prefab = unit.Stats.Type switch
            {
                UnitType.Player => UIPlayerPrefab,
                UnitType.Ally => UIAllyPrefab,
                UnitType.Enemy => UIEnemyPrefab,
                _ => null
            };
            
            // ������� ��������� ������� � ��������� ��� � ��������
            var instance = Instantiate(prefab, transform);
            var childTransform = instance.transform.GetChild(0);
            var portraitImage = childTransform.GetComponent<Image>();

            // ��������� ������� � ��������� ���������
            GetPortrait(unit.Stats.Name, portraitImage);

            // ��������� ������� � �������
            UIPlayerPortraits.Add(unit.Stats.Name + unit.Stats.ID, portraitImage);

            // ��������� ��� � �������
            UIBackground.Add(portraitImage, GetBackground(unit, portraitImage));
            
            

        }
    }

    protected virtual UIUnit GetBackground(Unit unit, Image playerPortrait)
    {
        var uiUnit = playerPortrait.GetComponentInParent<UIUnit>();
        uiUnit.Unit = unit;
        return uiUnit;
    } 

    public Image GetPlayerPortrait(Unit unit) =>
        UIPlayerPortraits.TryGetValue(unit.Stats.Name + unit.Stats.ID, out var portrait) ? portrait : null;

    public UIUnit GetPlayerBackground(Image playerPortrait) =>
        UIBackground.TryGetValue(playerPortrait, out var background) ? background : null;
}