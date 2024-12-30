using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LocationActionProvider))]
public class Bed : MonoBehaviour, IActionProvider
{
    public HashSet<AgentAction> GetActions(GoapAgent agent, Dictionary<string, AgentBelief> beliefs)
    {
        var locationBeliefName = GetComponent<LocationActionProvider>().LocationBeliefName;
        return new HashSet<AgentAction>
        {
            new AgentAction.Builder($"RestAt{name}Restorer{GetInstanceID()}")
                .WithStrategy(new IdleStrategy(4))
                .AddPrecondition(beliefs[locationBeliefName])
                .AddEffect(beliefs["AgentIsRested"])
                .Build()
        };
    }
}