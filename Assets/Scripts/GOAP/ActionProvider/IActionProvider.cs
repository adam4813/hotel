using System.Collections.Generic;

public interface IActionProvider
{
    public Dictionary<string, AgentBelief> GetBeliefs(GoapAgent agent)
    {
        return new Dictionary<string, AgentBelief>();
    }

    public HashSet<AgentAction> GetActions(GoapAgent agent, Dictionary<string, AgentBelief> beliefs);
}