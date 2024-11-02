using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private CameraControlsAction _cameraControlsAction;
    private InputAction _movementAction;
    private Transform _cameraTransform;

    // horizontal
    [SerializeField] private float maxSpeed = 5f;
    private float _speed = 0f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float damping = 15f;

    // vertical - zoom
    [SerializeField] private float stepSize = 2f;
    [SerializeField] private float zoomDampening = 7.5f;
    [SerializeField] private float zoomSensitive = 100f;
    [SerializeField] private float minHeight = 5f;
    [SerializeField] private float maxHeight = 50f;
    [SerializeField] private float zoomSpeed = 2f;

    // rotation
    [SerializeField] private float maxRotationSpeed = 1f;

    // screen edge motion
    [SerializeField] [Range(0f, 0.1f)] private float edgeTolerance = 0.05f; // Percentage of screen size
    [SerializeField] private bool edgeMoveEnabled = true;

    private Vector3 _targetPosition;

    private float _zoomHeight;

    private Vector3 _horizontalVelocity;
    private Vector3 _lastPosition;

    private Vector3 _startDrag;
    private Camera _camera;

    private void Awake()
    {
        _cameraControlsAction = new CameraControlsAction();
        _camera = GetComponentInChildren<Camera>();
        _cameraTransform = _camera.transform;
    }

    private void OnEnable()
    {
        _zoomHeight = _cameraTransform.localPosition.y;
        _cameraTransform.LookAt(transform);
        _lastPosition = transform.position;
        _movementAction = _cameraControlsAction.Camera.Movement;

        _cameraControlsAction.Camera.RotateCamera.performed += RotateCamera;
        _cameraControlsAction.Camera.ZoomCamera.performed += ZoomCamera;
        _cameraControlsAction.Camera.Enable();
    }

    private void OnDisable()
    {
        _cameraControlsAction.Camera.RotateCamera.performed -= RotateCamera;
        _cameraControlsAction.Camera.ZoomCamera.performed -= ZoomCamera;
        _cameraControlsAction.Disable();
    }

    private void Update()
    {
        GetKeyboardMovement();
        if (edgeMoveEnabled)
        {
            CheckMouseAtScreenEdge();
        }
        DragCamera();
        
        UpdateVelocity();
        UpdateCameraPosition();
        UpdateBasePosition();
    }

    private void UpdateVelocity()
    {
        _horizontalVelocity = (transform.position - _lastPosition) / Time.deltaTime;
        _horizontalVelocity.y = 0f;
        _lastPosition = transform.position;
    }

    private void GetKeyboardMovement()
    {
        var value = _movementAction.ReadValue<Vector2>();
        var movementDirection = value.x * GetCameraRight() + value.y * GetCameraForward();
        movementDirection = movementDirection.normalized;
        if (movementDirection.sqrMagnitude > 0.1f)
        {
            _targetPosition += movementDirection;
        }
    }

    private Vector3 GetCameraRight()
    {
        var right = _cameraTransform.right;
        right.y = 0f;
        return right;
    }

    private Vector3 GetCameraForward()
    {
        var forward = _cameraTransform.forward;
        forward.y = 0f;
        return forward;
    }

    private void UpdateBasePosition()
    {
        if (_targetPosition.sqrMagnitude > 0.1f)
        {
            _speed = Mathf.Lerp(_speed, maxSpeed, acceleration * Time.deltaTime);
            transform.position += _targetPosition * (_speed * Time.deltaTime);
        }
        else
        {
            _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, Vector3.zero, damping * Time.deltaTime);
            transform.position += _horizontalVelocity * Time.deltaTime;
        }

        _targetPosition = Vector3.zero;
    }

    private void RotateCamera(InputAction.CallbackContext inputValue)
    {
        if (!Mouse.current.middleButton.isPressed) return;

        var value = inputValue.ReadValue<Vector2>().x;
        transform.rotation = Quaternion.Euler(0f, value * maxRotationSpeed + transform.rotation.eulerAngles.y, 0f);
    }

    private void ZoomCamera(InputAction.CallbackContext inputValue)
    {
        var value = -inputValue.ReadValue<Vector2>().y / zoomSensitive;
        if (Mathf.Abs(value) < 0.1f) return;

        _zoomHeight = Mathf.Clamp(_cameraTransform.localPosition.y + value * stepSize, minHeight, maxHeight);
    }

    private void UpdateCameraPosition()
    {
        var zoomTarget = new Vector3(_cameraTransform.localPosition.x, _zoomHeight, _cameraTransform.localPosition.z);
        zoomTarget -= zoomSpeed * (_zoomHeight - _cameraTransform.localPosition.y) * Vector3.forward;

        _cameraTransform.localPosition =
            Vector3.Lerp(_cameraTransform.localPosition, zoomTarget, Time.deltaTime * zoomDampening);
        _cameraTransform.LookAt(transform);
    }

    private void CheckMouseAtScreenEdge()
    {
        var mousePosition = Mouse.current.position.ReadValue();
        var moveDirection = Vector3.zero;

        if (mousePosition.x < edgeTolerance * Screen.width)
            moveDirection += -GetCameraRight();
        else if (mousePosition.x > (1 - edgeTolerance) * Screen.width)
            moveDirection += GetCameraRight();

        if (mousePosition.y < edgeTolerance * Screen.height)
            moveDirection += -GetCameraForward();
        else if (mousePosition.y > (1 - edgeTolerance) * Screen.height)
            moveDirection += GetCameraForward();

        _targetPosition += moveDirection;
    }

    private void DragCamera()
    {
        if (!Mouse.current.rightButton.isPressed) return;

        var plane = new Plane(Vector3.up, Vector3.zero);
        var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!plane.Raycast(ray, out var distance)) return;

        var hitPoint = ray.GetPoint(distance);
        if (Mouse.current.rightButton.wasPressedThisFrame)
            _startDrag = hitPoint;
        else
            _targetPosition += _startDrag - hitPoint;
    }
}