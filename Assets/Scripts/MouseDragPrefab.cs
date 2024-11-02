using UnityEngine;
using UnityEngine.InputSystem;

public class MouseDragPrefab : MonoBehaviour
{
    private WorldMouseControls _worldMouseControls;
    private GameObject _prefab;

    private void Awake()
    {
        _worldMouseControls = new WorldMouseControls();
    }

    private void OnEnable()
    {
        _worldMouseControls.Enable();
        _worldMouseControls.Mouse.Position.performed += OnMouseMovePerformed;
    }

    private void OnDisable()
    {
        _worldMouseControls.Mouse.Position.performed -= OnMouseMovePerformed;
        _worldMouseControls.Disable();
    }

    private void OnMouseMovePerformed(InputAction.CallbackContext ctx)
    {
        if (_prefab == null) return;

        Vector3 mousePos = _worldMouseControls.Mouse.Position.ReadValue<Vector2>();
        if (Camera.main == null) return;

        var ray = Camera.main.ScreenPointToRay(mousePos);
        if (!Physics.Raycast(ray, out var hit, 1000)) return;

        var hotelFloor = hit.collider.gameObject.GetComponent<HotelFloor>();
        if (hotelFloor == null)
        {
            _prefab.gameObject.SetActive(false);
            return;
        }

        _prefab.gameObject.SetActive(true);
        _prefab.transform.position = hotelFloor.GetNearestPointOnGrid(hit.point);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void SetPrefab(GameObject prefab)
    {
        _prefab = Instantiate(prefab, transform);
        _prefab.gameObject.SetActive(false);
        foreach (var childCollider in _prefab.GetComponentsInChildren<Collider>())
        {
            childCollider.enabled = false;
        }
        gameObject.SetActive(true);
    }

    public void ClearPrefab()
    {
        if (_prefab)
        {
            Destroy(_prefab);
        }

        _prefab = null;
        gameObject.SetActive(false);
    }

    public bool HasPrefab()
    {
        return _prefab != null;
    }
}