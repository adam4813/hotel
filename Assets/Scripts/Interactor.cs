using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionRadius;
    [SerializeField] private LayerMask layerMask;

    private PlayerControls _playerControls;
    private PlayerControls.PlayerActions _playerActions;

    private readonly Collider[] _colliders = new Collider[3];
    private int numFound;
    private IInteractable _interactable;

    private void Awake()
    {
        _playerControls = new PlayerControls();
        _playerActions = _playerControls.Player;
    }
    
    private void OnEnable()
    {
        _playerControls.Enable();
        _playerActions.Interact.performed += OnInteractPerformed;
    }
    
    private void OnDisable()
    {
        _playerActions.Interact.performed -= OnInteractPerformed;
        _playerControls.Disable();
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        Interact();
    }

    private void Update()
    {
        numFound = Physics.OverlapSphereNonAlloc(interactionPoint.position, interactionRadius, _colliders, layerMask);
        _interactable = numFound > 0 ? _colliders[0].GetComponent<IInteractable>() : null;
    }

    private void Interact()
    {
        Debug.Log("interacting with " + _interactable);
        _interactable?.OnInteract(this);
    }
}