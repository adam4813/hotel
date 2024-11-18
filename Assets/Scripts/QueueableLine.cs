using System.Collections.Generic;
using UnityEngine;

public class QueueableLine : MonoBehaviour
{
    [SerializeField] private int maxQueueSize;
    private readonly Queue<GameObject> _queue = new();
    public bool IsQueueFull => _queue.Count >= maxQueueSize;

    public bool IsInQueue(GameObject agent) => _queue.Contains(agent);

    public bool AddToQueue(GameObject agent)
    {
        if (_queue.Count >= maxQueueSize) return false;
        _queue.Enqueue(agent);
        return true;
    }
    
    public GameObject GetNextInQueue()
    {
        return _queue.Count == 0 ? null : _queue.Dequeue();
    }
}