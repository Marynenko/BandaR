using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIGroupPortraits : MonoBehaviour
{
    public GameObject uiPlayerPrefab; // ������ UIPlayer
    public GameObject uiEnemyPrefab; // ������ UIEnemy
    public Dictionary<string, Image> uiPlayerPortraits = new();
    public Dictionary<Image, UIUnit> uiBackground = new();

    public void InitializePortrait(ref Unit unit)
    {
        Unit.OnMove += UpdatePortraits;
        unit.Portrait = GetPlayerPortrait(unit);
        // GetPortrait(unit.Stats.Name, playerPortrait);
        // uiPlayerPortraits.Add(unit.Stats.Name + unit.Stats.ID, playerPortrait);
        Debug.Log(unit.Stats.Name + unit.Stats.ID);
        // uiBackground.Add(playerPortrait, GetBackground(playerPortrait));
    }

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
    private UIUnit GetBackground(Image playerPortrait) => playerPortrait.GetComponentInParent<UIUnit>();

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
            GameObject prefab;

            // �������� ������ ������ � ����������� �� ���� �����
            if (unit.Stats.Type == UnitType.Player)
            {
                prefab = uiPlayerPrefab;
            }
            else if (unit.Stats.Type == UnitType.Enemy)
            {
                prefab = uiEnemyPrefab;
            }
            else
            {
                continue;
            }

            // ������� ��������� ������� � ��������� ��� � ��������
            var instance = Instantiate(prefab, transform);
            var childTransform = instance.transform.GetChild(0);
            var portraitImage = childTransform.GetComponent<Image>();
        
            // ��������� ������� � ��������� ���������
            GetPortrait(unit.Stats.Name, portraitImage);

            // ��������� ������� � �������
            uiPlayerPortraits.Add(unit.Stats.Name + unit.Stats.ID, portraitImage);

            // ��������� ��� � �������
            uiBackground.Add(portraitImage, GetBackground(portraitImage));
        }
    }

    
    public Image GetPlayerPortrait(Unit unit) =>
        uiPlayerPortraits.TryGetValue(unit.Stats.Name + unit.Stats.ID, out var portrait) ? portrait : null;
    public UIUnit GetPlayerBackground(Image playerPortrait) =>
        uiBackground.TryGetValue(playerPortrait, out var background) ? background : null;
    
    
}