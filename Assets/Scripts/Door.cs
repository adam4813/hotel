using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
public class Door : MonoBehaviour
{
    private NavMeshObstacle _navMeshObstacle;

    public bool IsOpen;

    [SerializeField] private float speed = 1.0f;
    [SerializeField] private float rotationAmount = 90.0f;
    [SerializeField] private float forwardDirection = 0;

    private Vector3 _startRotation;
    private Vector3 _forward;

    private Coroutine _animationCoroutine;


    private void Awake()
    {
        _navMeshObstacle = GetComponent<NavMeshObstacle>();
        _navMeshObstacle.carveOnlyStationary = false;
        _navMeshObstacle.carving = IsOpen;
        _navMeshObstacle.enabled = IsOpen;
        _startRotation = transform.rotation.eulerAngles;

        _forward = transform.forward;
    }

    public void Open(Vector3 playerPosition)
    {
        if (IsOpen) return;
        
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        var dot = Vector3.Dot(_forward, (playerPosition - transform.position).normalized);

        _animationCoroutine = StartCoroutine(OpenDoor(dot));
    }

    private IEnumerator OpenDoor(float forwardAmount)
    {
        var startRotation = transform.rotation;
        var endRotation = Quaternion.Euler(forwardAmount >= forwardDirection
            ? new Vector3(startRotation.x, startRotation.y + rotationAmount, startRotation.z)
            : new Vector3(startRotation.x, startRotation.y - rotationAmount, startRotation.z));

        IsOpen = true;

        float time = 0;
        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
            yield return null;
            time += Time.deltaTime * speed;
        }

        _navMeshObstacle.carving = true;
        _navMeshObstacle.enabled = true;
    }

    public void Close()
    {
        if (!IsOpen) return;
        
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        _animationCoroutine = StartCoroutine(CloseDoor());
    }

    private IEnumerator CloseDoor()
    {
        _navMeshObstacle.carving = false;
        _navMeshObstacle.enabled = false;
        
        var startRotation = transform.rotation;
        var endRotation = Quaternion.Euler(_startRotation);

        IsOpen = false;

        float time = 0;
        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, time);
            yield return null;
            time += Time.deltaTime * speed;
        }
    }
}