using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QueueableLine : MonoBehaviour, IActionProvider
{
    [SerializeField] private int maxQueueSize;
    [SerializeField] private LocationActionProvider queueExit;
    private readonly List<GameObject> queueList = new();
    public List<GameObject> GetQueue() => queueList;
    public bool IsQueueFull => queueList.Count >= maxQueueSize;
    public bool IsQueueEmpty => queueList.Count == 0;
    public bool IsInQueue(GameObject agent) => queueList.Contains(agent);

    public bool AddToQueue(GameObject agent)
    {
        if (queueList.Count >= maxQueueSize) return false;
        queueList.Add(agent);
        return true;
    }

    public void RemoveFromQueue(GameObject go)
    {
        queueList.Remove(go);
    }

    public GameObject GetNextInQueue()
    {
        return queueList.Count == 0 ? null : queueList.First();
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