using System.Collections.Generic;
using UnityEngine;

public class LocationActionProvider : MonoBehaviour, IActionProvider
{
    [SerializeField] private float radius;

    private string UniqueName => $"{name}Location{GetInstanceID()}";
    public string LocationBeliefName => $"AgentAt{UniqueName}";

    public void AddBeliefs(BeliefFactory factory)
    {
        factory.AddLocationBelief(LocationBeliefName, radius, transform.position);
    }

    public HashSet<AgentAction> GetActions(GoapAgent agent, Dictionary<string, AgentBelief> beliefs)
    {
        return new HashSet<AgentAction>
        {
            new AgentAction.Builder($"MoveTo{UniqueName}")
                .WithStrategy(new MoveStrategy(agent.NavMeshAgent, () => transform.position))
                .AddEffect(beliefs[LocationBeliefName])
                .Build()
        };
    }
}