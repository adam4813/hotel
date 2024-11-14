using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    private PlayerControls _playerControls;
    private PlayerControls.PlayerActions _playerActions;
    
    private Vector2 _movementInput;

    private void Awake()
    {
        _playerControls = new PlayerControls();
        _playerActions = _playerControls.Player;
    }

    private void OnMovementStopped(InputAction.CallbackContext ctx)
    {
        _movementInput = Vector2.zero;
    }

    private void OnMovementPerformed(InputAction.CallbackContext ctx)
    {
        _movementInput = ctx.ReadValue<Vector2>();
    }

    private void OnEnable()
    {
        _playerControls.Enable();
        
        _playerActions.Movement.performed += OnMovementPerformed;
        _playerActions.Movement.canceled += OnMovementStopped;
    }
    
    private void OnDisable()
    {
        _playerActions.Movement.performed -= OnMovementPerformed;
        _playerActions.Movement.canceled -= OnMovementStopped;
        
        _playerControls.Disable();
    }
    
    private void Update()
    {
        playerMovement.SetMovementInput(_movementInput);
    }
}
