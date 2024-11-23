using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LocationActionProvider))]
public class QueueableLine : MonoBehaviour, IActionProvider
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
    
    public string QueueBeliefName => $"AgentIn{name}Queue";

    public Dictionary<string, AgentBelief> GetBeliefs(GoapAgent agent)
    {
        var beliefs = new Dictionary<string, AgentBelief>();
        var beliefFactory = new BeliefFactory(agent, beliefs);
        beliefFactory.AddBelief(QueueBeliefName, () => IsInQueue(agent.gameObject));
        return beliefs;
    }

    public HashSet<AgentAction> GetActions(GoapAgent agent, Dictionary<string, AgentBelief> beliefs)
    {
        var locationBeliefName = GetComponent<LocationActionProvider>().LocationBeliefName;
        return new HashSet<AgentAction>
        {
            new AgentAction.Builder($"GetIn{name}Queue")
                .WithStrategy(new QueueInLineStrategy(this, agent.gameObject))
                .AddPrecondition(beliefs[locationBeliefName])
                .AddEffect(beliefs[QueueBeliefName])
                .Build()
        };
    }
}