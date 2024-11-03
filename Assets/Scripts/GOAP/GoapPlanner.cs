using System.Collections.Generic;
using System.Linq;

public interface IGoapPlanner
{
    ActionPlan Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null);
}

public class ActionPlanNode
{
    public ActionPlanNode Parent { get; }
    public AgentAction Action { get; }
    public HashSet<AgentBelief> RequiredEffects { get; }
    public List<ActionPlanNode> Leaves { get; }
    public float Cost { get; }

    public bool IsLeafDead => Leaves.Count == 0 && Action == null;

    public ActionPlanNode(ActionPlanNode parent, AgentAction action, HashSet<AgentBelief> effects, float cost)
    {
        Parent = parent;
        Action = action;
        RequiredEffects = new HashSet<AgentBelief>(effects);
        Leaves = new List<ActionPlanNode>();
        Cost = cost;
    }
}

public class GoapPlanner : IGoapPlanner
{
    public ActionPlan Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null)
    {
        // order goals by priority descending
        var orderedGoals = goals
            .Where(goal => goal.DesiredEffects.Any(effect => !effect.Evaluate()))
            .OrderByDescending(goal => goal == mostRecentGoal ? goal.Priority - 0.01 : goal.Priority)
            .ToList();

        // try to plan for each goal
        foreach (var goal in orderedGoals)
        {
            var goalNode = new ActionPlanNode(null, null, goal.DesiredEffects, 0);

            if (!FindPath(goalNode, agent.Actions) || goalNode.IsLeafDead) continue;
                
            var actionStack = new Stack<AgentAction>();
            while (goalNode.Leaves.Count > 0)
            {
                var cheapestLeaf = goalNode.Leaves.OrderBy(leaf => leaf.Cost).First();
                goalNode = cheapestLeaf;
                actionStack.Push(cheapestLeaf.Action);
            }
                
            return new ActionPlan(goal, actionStack, goalNode.Cost);
        }

        return null;
    }

    private bool FindPath(ActionPlanNode parent, HashSet<AgentAction> actions)
    {
        var orderedActions = actions.OrderBy(action => action.Cost);
        
        foreach (var action in orderedActions)
        {
            var requiredEffects = parent.RequiredEffects;
            requiredEffects.RemoveWhere(effect => effect.Evaluate());

            if (requiredEffects.Count == 0) return true;
            if (!action.Effects.Any(requiredEffects.Contains)) continue;

            var newRequiredEffects = new HashSet<AgentBelief>(requiredEffects);
            newRequiredEffects.ExceptWith(action.Effects);
            newRequiredEffects.UnionWith(action.Preconditions);

            var newAvailableActions = new HashSet<AgentAction>(actions);
            newAvailableActions.Remove(action);

            var newNode = new ActionPlanNode(parent, action, newRequiredEffects, parent.Cost + action.Cost);

            if (FindPath(newNode, newAvailableActions))
            {
                parent.Leaves.Add(newNode);
                newRequiredEffects.ExceptWith(newNode.Action.Preconditions);
            }

            if (newRequiredEffects.Count == 0) return true;
        }

        return false;
    }
}