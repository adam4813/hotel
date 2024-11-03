using System.Collections.Generic;

public class ActionPlan
{
    public AgentGoal Goal { get; }
    public Stack<AgentAction> Actions { get; }
    public float TotalCost { get; set; }

    public ActionPlan(AgentGoal goal, Stack<AgentAction> actions, float totalCost)
    {
        Goal = goal;
        Actions = actions;
        TotalCost = totalCost;
    }
}