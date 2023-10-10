using UnityEngine;
using UnityEngine.UI;


public class UIUnit : MonoBehaviour
{
    [SerializeField] private Image Image;

    private void Start()
    {
        if (Image != null) return;
        Image = GetComponent<Image>();
    }

    public void TurnOnAlpha()
    {
        var color = Image.color;
        color.a = 1;
        Image.color = color;
    }

    public void TurnOffAlpha()
    {
        var color = Image.color;
        color.a = 0;
        Image.color = color;
    }
}
