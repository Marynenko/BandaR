using UnityEngine;
using Cursor = UnityEngine.Cursor;

public class CameraManager : MonoBehaviour
{
    // Нужно для начала создать родителя камеры
    [SerializeField] private Camera MainCamera; // Можно класть камеру
    public float Speed = 5f; // Скорость движение
    public bool IsActive = true;
    
    private float _speedMouse = 2; // Скорость движения на мыши.
    private bool _isRotateCam; // bool для проверки врощение мышки
    private bool _isMovingCam; // bool для проверки движение мышки
    private bool _changeMoving; // bool для замены движение

    private void Update()
    {
        if (!IsActive) return;
        // Приближение Отдаление колёсиком мышки
        var mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        MainCamera.transform.Translate(Vector3.forward * (mouseScroll * Speed));

        ChangeMove(); // Запуск ChangeMove()
        MouseRotate(); // Запуск MouseRotate()
    }
    
    private void ChangeMove()
    {
        if (Input.GetKeyDown(KeyCode.G)) // при нажатии на кнупку G меняется вид движение
        {
            _changeMoving = !_changeMoving;
        }

        if (_changeMoving)
        {
            MouseMoving(); // Запуск MouseMoving() Движение от колёсико мышки
        }

        if (!_changeMoving)
        {
            KeyBoardMoving(); // Запуск MouseMoving() Движение от колёсико мышки
        }
    }

    private void KeyBoardMoving() // Движение на кловиатуре на буквы WASD
    {
        var horizontalMove = -Input.GetAxis("Horizontal");
        var verticalMove = Input.GetAxis("Vertical");

        transform.Translate(Vector3.left * (Speed * horizontalMove * Time.deltaTime));
        transform.Translate(Vector3.forward * (Speed * verticalMove * Time.deltaTime));
    }

    private void MouseMoving() // Движение на Мышке(Колёсико)
    {
        if (Input.GetMouseButtonDown(2)) // Проверка нажата ли колёсико мышки
        {
            _isMovingCam = true;
            Cursor.lockState = CursorLockMode.Locked; // Скрывает курсор
        }

        if (Input.GetMouseButtonUp(2)) // Проверка отпущена ли колёсико мышки
        {
            _isMovingCam = false;
            Cursor.lockState = CursorLockMode.None; // Показывает курсор
        }

        if (_isMovingCam) // Движение
        {
            var mouseX = Input.GetAxis("Mouse X"); // если хочешь поменять ставь минус перед Input
            var mouseY = Input.GetAxis("Mouse Y"); // если хочешь поменять ставь минус перед Input

            transform.Translate(Vector3.forward * (Speed * mouseY * Time.deltaTime));
            transform.Translate(Vector3.left * (Speed * mouseX * Time.deltaTime));
        }
    }

    private void MouseRotate() // Врощение мышки
    {
        if (Input.GetMouseButtonDown(1)) //Проверка нажата ли правая кнопка мыши
        {
            _isRotateCam = true;
            Cursor.lockState = CursorLockMode.Locked; // Скрывает курсор
        }

        if (Input.GetMouseButtonUp(1)) //Проверка отпущена ли правая кнопка мыши
        {
            _isRotateCam = false;
            Cursor.lockState = CursorLockMode.None; // Показывает курсор
        }

        if (_isRotateCam) // Врощяет камеру по X
        {
            var mouseX = Input.GetAxis("Mouse X");
            transform.Rotate(Vector3.up * (_speedMouse * mouseX));
        }
    }
}