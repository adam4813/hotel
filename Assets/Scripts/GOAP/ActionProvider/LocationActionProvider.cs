using System.Collections.Generic;
using UnityEngine;

public class LocationActionProvider : MonoBehaviour, IActionProvider
{
    [SerializeField] private float radius;
    public string LocationBeliefName => $"AgentAt{name}Location";

    public Dictionary<string, AgentBelief> GetBeliefs(GoapAgent agent)
    {
        var beliefs = new Dictionary<string, AgentBelief>();
        var beliefFactory = new BeliefFactory(agent, beliefs);
        beliefFactory.AddLocationBelief(LocationBeliefName, radius, transform);
        return beliefs;
    }
    
    public HashSet<AgentAction> GetActions(GoapAgent agent, Dictionary<string, AgentBelief> beliefs)
    {
        return new HashSet<AgentAction>
        {
            new AgentAction.Builder($"MoveTo{name}")
                .WithStrategy(new MoveStrategy(agent.NavMeshAgent, () => transform.position))
                .AddEffect(beliefs[LocationBeliefName])
                .Build()
        };
    }
}