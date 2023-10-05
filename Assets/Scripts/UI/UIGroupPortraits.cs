using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIGroupPortraits : MonoBehaviour
{
    public Dictionary<string, Image> uiPlayerPortraits = new();
    public Dictionary<Image, UIUnit> uiBackground = new();

    public void InitializePortrait(Unit unit, Image playerPortrait)
    {
        GetPortrait(unit.Name, playerPortrait);
        uiPlayerPortraits.Add(unit.Name, playerPortrait);
        uiBackground.Add(playerPortrait, GetBackground(playerPortrait));
    }

    // Вызывается для загрузки портрета по имени из ресурсов
    private void GetPortrait(string unitName, Image portraitImage)
    {
        // Попробуйте загрузить текстуру из ресурсов
        var portraitTexture = Resources.Load<Texture2D>("Portraits/" + unitName);
        if (portraitTexture == null) return;
        var rect = new Rect(0, 0, portraitTexture.width, portraitTexture.height);
        // Установите текстуру как изображение для компонента Image
        portraitImage.sprite = Sprite.Create(portraitTexture, rect, new Vector2(.5f, .5f));
    }

    private UIUnit GetBackground(Image playerPortrait)
    {
        return playerPortrait.GetComponentInParent<UIUnit>();
    }

    public Image GetPlayerPortrait(Unit unit)
    {
        try
        {
            return uiPlayerPortraits.TryGetValue(unit.Name, out var portrait) ? portrait : null;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    public UIUnit GetPlayerBackground(Image playerPortrait)
    {
        try
        {
            return uiBackground.TryGetValue(playerPortrait, out var background) ? background : null;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }
}