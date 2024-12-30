using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    private CharacterController characterController;
    private Vector2 _movementInput;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void SetMovementInput(Vector2 movementInput)
    {
        _movementInput = movementInput;
    }
    
    private void Update()
    {
        var velocity = (Vector3.right * _movementInput.x + Vector3.forward * _movementInput.y);
        velocity.y = 0;
        characterController.Move(velocity * (speed * Time.deltaTime));
        if (velocity != Vector3.zero)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(velocity), rotationSpeed * Time.deltaTime);
        }

        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }
}
