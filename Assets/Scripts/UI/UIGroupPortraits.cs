using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGroupPortraits : MonoBehaviour
{
    public Dictionary<string, Image> PlayerPortraits = new Dictionary<string, Image>();

    public void InitializePortrait(Unit unit, Image portraitImage)
    {
        GetPortrait(unit.Name, portraitImage);
        PlayerPortraits.Add(unit.Name, portraitImage);
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
    //
    // private Image GetPlayerPortrait(Unit unit)
    // {
    //     if (PlayerPortraits.TryGetValue(unit.name, out var portrait))
    //     {
    //         return portrait;
    //     }
    //     else
    //     {
    //         // ������� null ��� ����������� �������, ���� ��� ������ �� ������ �������
    //         return null;
    //     }
    // }
}
