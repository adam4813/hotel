using System.Collections.Generic;

public interface IActionProvider
{
    public void AddBeliefs(BeliefFactory factory)
    {
    }

    public HashSet<AgentAction> GetActions(GoapAgent agent, Dictionary<string, AgentBelief> beliefs);
}