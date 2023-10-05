using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class UIUnit : MonoBehaviour
{
    [SerializeField] private Image image;

    private void Start()
    {
        if (image != null) return;
        image = GetComponent<Image>();
    }

    public void TurnOnAlpha()
    {
        var color = image.color;
        color.a = 1;
        image.color = color;
    }

    public void TurnOffAlpha()
    {
        var color = image.color;
        color.a = 0;
        image.color = color;
    }
}
