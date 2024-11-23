public class WaitForBeliefStrategy : IActionStrategy
{
    public bool CanPerform => true;
    public bool Complete => _belief.Evaluate();

    private readonly AgentBelief _belief;

    public WaitForBeliefStrategy(AgentBelief belief)
    {
        _belief = belief;
    }
}