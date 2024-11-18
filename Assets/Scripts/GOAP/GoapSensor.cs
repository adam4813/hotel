using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class GoapSensor : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float timerInterval = 1f;
    [SerializeField] private bool chaseSensor;
    
    private SphereCollider _collider;

    public event Action OnTargetChanged = delegate { };

    public Vector3 TargetPosition => _target != null ? _target.transform.position : Vector3.zero;
    public bool IsTargetInRange => TargetPosition != Vector3.zero;
    public bool IsChaseSensor => chaseSensor;

    private GameObject _target;
    private Vector3 _lastPosition;
    private CountdownTimer _timer;

    private void Awake()
    {
        _collider = GetComponent<SphereCollider>();
        _collider.isTrigger = true;
        _collider.radius = detectionRadius;
    }

    private void Start()
    {
        _timer = new CountdownTimer(timerInterval);
        _timer.OnTimerStop += () =>
        {
            UpdateTargetPosition(_target);
            _timer.Start();
        };
        _timer.Start();
    }

    private void Update()
    {
        _timer.Tick(Time.deltaTime);
    }

    private void UpdateTargetPosition(GameObject target = null)
    {
        _target = target;
        if (IsTargetInRange && (_lastPosition != TargetPosition || _lastPosition != Vector3.zero))
        {
            _lastPosition = TargetPosition;
            OnTargetChanged?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        UpdateTargetPosition(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        UpdateTargetPosition();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsTargetInRange ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}