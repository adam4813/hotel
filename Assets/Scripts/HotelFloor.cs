using UnityEngine;
using UnityEngine.InputSystem;

public class HotelFloor : MonoBehaviour
{
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private GameObject floorPlane;

    private WorldMouseControls _worldMouseControls;

    private void Awake()
    {
        _worldMouseControls = new WorldMouseControls();
    }

    private void OnClickOnPerformed(InputAction.CallbackContext ctx)
    {
        var gameManager = GameManager.Instance;
        
        if (!gameManager.MouseDragPrefab.HasPrefab()) return;
        
        Vector3 mousePos = _worldMouseControls.Mouse.Position.ReadValue<Vector2>();
        if (Camera.main == null) return;

        var ray = Camera.main.ScreenPointToRay(mousePos);
        if (!Physics.Raycast(ray, out var hit, 1000) || hit.collider != floorPlane.GetComponent<Collider>()) return;

        var instance = Instantiate(gameManager.ActivePrefab, floorPlane.transform);
        instance.transform.position = GetNearestPointOnGrid(hit.point);
        
        gameManager.MouseDragPrefab.ClearPrefab();
    }

    public Vector3 GetNearestPointOnGrid(Vector3 position)
    {
        position -= floorPlane.transform.position;
        
        var x = Mathf.RoundToInt(position.x / gridSize) * gridSize;
        var y = Mathf.RoundToInt(position.y / gridSize) * gridSize;
        var z = Mathf.RoundToInt(position.z / gridSize) * gridSize;

        return new Vector3(x, y, z) + floorPlane.transform.position;
    }

    private void OnEnable()
    {
        _worldMouseControls.Enable();
        _worldMouseControls.Mouse.Click.performed += OnClickOnPerformed;
    }

    private void OnDisable()
    {
        _worldMouseControls.Mouse.Click.performed -= OnClickOnPerformed;
        _worldMouseControls.Disable();
    }
}