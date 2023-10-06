using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIGroupPortraits : MonoBehaviour
{
    public GameObject uiPlayerPrefab; // Префаб UIPlayer
    public GameObject uiEnemyPrefab; // Префаб UIEnemy
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
    private UIUnit GetBackground(Image playerPortrait) => playerPortrait.GetComponentInParent<UIUnit>();

    private void UpdatePortraits(Unit unit)
    {
        // Получаем портрет игрока
        var playerPortrait = GetPlayerPortrait(unit);

        // Если портрет не найден, то ничего не делаем
        if (playerPortrait == null) return;

        // Получаем фон портрета
        var playerBackground = GetPlayerBackground(playerPortrait);

        // Если фон не найден, то ничего не делаем
        if (playerBackground == null) return;

        // Обновляем портрет и фон в соответствии с новым положением игрока
        // Здесь вы можете добавить свой код для обновления портрета и фона
    }
    
    public void AddPortraits(List<Unit> units)
    {
        foreach (var unit in units)
        {
            GameObject prefab;

            // Выбираем нужный префаб в зависимости от типа юнита
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

            // Создаем экземпляр префаба и добавляем его в иерархию
            var instance = Instantiate(prefab, transform);
            var childTransform = instance.transform.GetChild(0);
            var portraitImage = childTransform.GetComponent<Image>();
        
            // Загружаем портрет в созданный экземпляр
            GetPortrait(unit.Stats.Name, portraitImage);

            // Добавляем портрет в словарь
            uiPlayerPortraits.Add(unit.Stats.Name + unit.Stats.ID, portraitImage);

            // Добавляем фон в словарь
            uiBackground.Add(portraitImage, GetBackground(portraitImage));
        }
    }

    
    public Image GetPlayerPortrait(Unit unit) =>
        uiPlayerPortraits.TryGetValue(unit.Stats.Name + unit.Stats.ID, out var portrait) ? portrait : null;
    public UIUnit GetPlayerBackground(Image playerPortrait) =>
        uiBackground.TryGetValue(playerPortrait, out var background) ? background : null;
    
    
}