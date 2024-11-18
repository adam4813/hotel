using System.Collections.Generic;

public class AgentAction
{
    public string Name { get; }
    public float Cost { get; private set; }

    public HashSet<AgentBelief> Preconditions { get; } = new();
    public HashSet<AgentBelief> Effects { get; } = new();

    private IActionStrategy _strategy;
    public bool Complete => _strategy.Complete;

    private AgentAction(string name)
    {
        Name = name;
    }

    public void Start() => _strategy.Start();

    public void Update(float deltaTime)
    {
        if (_strategy.CanPerform)
        {
            _strategy.Update(deltaTime);
        }

        if (!_strategy.Complete) return;

        foreach (var effect in Effects)
        {
            effect.Evaluate();
        }
    }

    public void Stop() => _strategy.Stop();
    
    public class Builder
    {
        private readonly AgentAction _action;

        public Builder(string name)
        {
            _action = new AgentAction(name)
            {
                Cost = 1
            };
        }
        
        public Builder WithCost(float cost)
        {
            _action.Cost = cost;
            return this;
        }
        
        public Builder WithStrategy(IActionStrategy strategy)
        {
            _action._strategy = strategy;
            return this;
        }
        
        public Builder AddPrecondition(AgentBelief belief)
        {
            _action.Preconditions.Add(belief);
            return this;
        }
        
        public Builder AddEffect(AgentBelief belief)
        {
            _action.Effects.Add(belief);
            return this;
        }

        public AgentAction Build() => _action;
    }
}