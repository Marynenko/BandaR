using System;
using UnityEngine;
using UnityEngine.UI;


public class UIUnit : MonoBehaviour
{
    [SerializeField] private Image _image;

    private void Start()
    {
        if (_image != null) return;
        _image = GetComponent<Image>();
    }

    public void TurnOnAlpha()
    {
        var color = _image.color;
        color.a = 1;
        _image.color = color;
    }

    public void TurnOffAlpha()
    {
        var color = _image.color;
        color.a = 0;
        _image.color = color;
    }
}
