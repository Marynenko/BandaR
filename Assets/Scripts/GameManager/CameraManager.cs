using UnityEngine;
using Cursor = UnityEngine.Cursor;

public class CameraManager : MonoBehaviour
{
    // ����� ��� ������ ������� �������� ������
    [SerializeField] private Camera MainCamera; // ����� ������ ������
    public float Speed = 5f; // �������� ��������
    public bool IsActive = true;
    
    private float _speedMouse = 2; // �������� �������� �� ����.
    private bool _isRotateCam; // bool ��� �������� �������� �����
    private bool _isMovingCam; // bool ��� �������� �������� �����
    private bool _changeMoving; // bool ��� ������ ��������

    private void Update()
    {
        if (!IsActive) return;
        // ����������� ��������� �������� �����
        var mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        MainCamera.transform.Translate(Vector3.forward * (mouseScroll * Speed));

        ChangeMove(); // ������ ChangeMove()
        MouseRotate(); // ������ MouseRotate()
    }
    
    private void ChangeMove()
    {
        if (Input.GetKeyDown(KeyCode.G)) // ��� ������� �� ������ G �������� ��� ��������
        {
            _changeMoving = !_changeMoving;
        }

        if (_changeMoving)
        {
            MouseMoving(); // ������ MouseMoving() �������� �� ������� �����
        }

        if (!_changeMoving)
        {
            KeyBoardMoving(); // ������ MouseMoving() �������� �� ������� �����
        }
    }

    private void KeyBoardMoving() // �������� �� ���������� �� ����� WASD
    {
        var horizontalMove = -Input.GetAxis("Horizontal");
        var verticalMove = Input.GetAxis("Vertical");

        transform.Translate(Vector3.left * (Speed * horizontalMove * Time.deltaTime));
        transform.Translate(Vector3.forward * (Speed * verticalMove * Time.deltaTime));
    }

    private void MouseMoving() // �������� �� �����(�������)
    {
        if (Input.GetMouseButtonDown(2)) // �������� ������ �� ������� �����
        {
            _isMovingCam = true;
            Cursor.lockState = CursorLockMode.Locked; // �������� ������
        }

        if (Input.GetMouseButtonUp(2)) // �������� �������� �� ������� �����
        {
            _isMovingCam = false;
            Cursor.lockState = CursorLockMode.None; // ���������� ������
        }

        if (_isMovingCam) // ��������
        {
            var mouseX = Input.GetAxis("Mouse X"); // ���� ������ �������� ����� ����� ����� Input
            var mouseY = Input.GetAxis("Mouse Y"); // ���� ������ �������� ����� ����� ����� Input

            transform.Translate(Vector3.forward * (Speed * mouseY * Time.deltaTime));
            transform.Translate(Vector3.left * (Speed * mouseX * Time.deltaTime));
        }
    }

    private void MouseRotate() // �������� �����
    {
        if (Input.GetMouseButtonDown(1)) //�������� ������ �� ������ ������ ����
        {
            _isRotateCam = true;
            Cursor.lockState = CursorLockMode.Locked; // �������� ������
        }

        if (Input.GetMouseButtonUp(1)) //�������� �������� �� ������ ������ ����
        {
            _isRotateCam = false;
            Cursor.lockState = CursorLockMode.None; // ���������� ������
        }

        if (_isRotateCam) // ������� ������ �� X
        {
            var mouseX = Input.GetAxis("Mouse X");
            transform.Rotate(Vector3.up * (_speedMouse * mouseX));
        }
    }
}