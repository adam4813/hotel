using UnityEngine;

public class QueueInLineStrategy : IActionStrategy
{
    private readonly QueueableLine _queueableLine;
    private readonly GameObject _agentGameObject;

    public bool CanPerform => !Complete && !_queueableLine.IsQueueFull;
    public bool Complete => _queueableLine.IsInQueue(_agentGameObject);

    public QueueInLineStrategy(QueueableLine queueableLine, GameObject agentGameObject)
    {
        _queueableLine = queueableLine;
        _agentGameObject = agentGameObject;
    }

    public void Start()
    {
        Debug.Log($"Trying to put {_agentGameObject.name} in queue.");
        
        if (_queueableLine.IsInQueue(_agentGameObject) || _queueableLine.IsQueueFull) return;

        _queueableLine.AddToQueue(_agentGameObject);
        
        Debug.Log($"{_agentGameObject.name} is now in the queue.");
    }
}