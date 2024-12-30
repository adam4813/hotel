using System;
using System.Collections.Generic;
using UnityEngine;

public class BeliefFactory
{
    public GoapAgent Agent { get; }

    private readonly Dictionary<string, AgentBelief> _beliefs;

    public BeliefFactory(GoapAgent agent, Dictionary<string, AgentBelief> beliefs)
    {
        Agent = agent;
        _beliefs = beliefs;
    }

    public void AddBelief(string key, Func<bool> condition)
    {
        _beliefs[key] = new AgentBelief.Builder(key)
            .WithCondition(condition)
            .Build();
    }

    public void AddSensorBelief(string key, GoapSensor goapSensor)
    {
        _beliefs[key] = new AgentBelief.Builder(key)
            .WithCondition(() => goapSensor.IsTargetInRange)
            .WithLocation(() => goapSensor.TargetPosition)
            .Build();
    }

    public void AddLocationBelief(string key, float distance, Vector3 position)
    {
        _beliefs[key] = new AgentBelief.Builder(key)
            .WithCondition(() => InRangeOf(position, distance))
            .WithLocation(() => position)
            .Build();
    }

    private bool InRangeOf(Vector3 position, float distance)
    {
        return Vector3.Distance(Agent.transform.position, position) < distance;
    }
}

public class AgentBelief
{
    public string Name { get; }

    private Func<bool> _condition = () => false;
    private Func<Vector3> _observedLocation = () => Vector3.zero;

    private AgentBelief(string name)
    {
        Name = name;
    }

    public bool Evaluate() => _condition();

    public class Builder
    {
        private readonly AgentBelief _belief;

        public Builder(string name)
        {
            _belief = new AgentBelief(name);
        }

        public Builder WithCondition(Func<bool> condition)
        {
            _belief._condition = condition;
            return this;
        }

        public Builder WithLocation(Func<Vector3> observedLocation)
        {
            _belief._observedLocation = observedLocation;
            return this;
        }

        public AgentBelief Build()
        {
            return _belief;
        }
    }
}