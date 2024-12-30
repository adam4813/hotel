using System.Collections.Generic;
using UnityEngine;

public class QueueableLine : MonoBehaviour, IActionProvider
{
    [SerializeField] private int maxQueueSize;
    [SerializeField] private LocationActionProvider queueExit;
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
    
    private string UniqueName => $"{name}Queue{GetInstanceID()}";
    public string QueueBeliefName => $"AgentIn{UniqueName}";

    public void AddBeliefs(BeliefFactory factory)
    {
        factory.AddBelief(QueueBeliefName,() => IsInQueue(factory.Agent.gameObject));
    }

    public HashSet<AgentAction> GetActions(GoapAgent agent, Dictionary<string, AgentBelief> beliefs)
    {
        var locationBeliefName = queueExit.LocationBeliefName;
        return new HashSet<AgentAction>
        {
            new AgentAction.Builder($"GetIn{UniqueName}")
                .WithStrategy(new QueueInLineStrategy(this, agent.gameObject))
                .AddPrecondition(beliefs[locationBeliefName])
                .AddEffect(beliefs[QueueBeliefName])
                .Build()
        };
    }
}