using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPortraitManager : MonoBehaviour
{
    public GameObject UIPlayerPrefab; // Префаб UIPlayer
    public GameObject UIEnemyPrefab; // Префаб UIEnemy
    public GameObject UIAllyPrefab; // Префаб UIPlayer
    public Dictionary<string, Image> UIPlayerPortraits = new();
    public Dictionary<Image, UIUnit> UIBackground = new();

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

            switch (unit.Stats.Type)
            {
                // Выбираем нужный префаб в зависимости от типа юнита
                case UnitType.Player:
                    prefab = UIPlayerPrefab;
                    break;
                case UnitType.Enemy:
                    prefab = UIEnemyPrefab;
                    break;
                case UnitType.Ally:
                    prefab = UIAllyPrefab;
                    break;
                default:
                    continue;
            }

            // Создаем экземпляр префаба и добавляем его в иерархию
            var instance = Instantiate(prefab, transform);
            var childTransform = instance.transform.GetChild(0);
            var portraitImage = childTransform.GetComponent<Image>();

            // Загружаем портрет в созданный экземпляр
            GetPortrait(unit.Stats.Name, portraitImage);

            // Добавляем портрет в словарь
            UIPlayerPortraits.Add(unit.Stats.Name + unit.Stats.ID, portraitImage);

            // Добавляем фон в словарь
            UIBackground.Add(portraitImage, GetBackground(portraitImage));
        }
    }

    protected virtual UIUnit GetBackground(Image playerPortrait) => playerPortrait.GetComponentInParent<UIUnit>();

    public Image GetPlayerPortrait(Unit unit) =>
        UIPlayerPortraits.TryGetValue(unit.Stats.Name + unit.Stats.ID, out var portrait) ? portrait : null;

    public UIUnit GetPlayerBackground(Image playerPortrait) =>
        UIBackground.TryGetValue(playerPortrait, out var background) ? background : null;
}