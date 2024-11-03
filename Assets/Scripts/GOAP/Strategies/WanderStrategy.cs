using UnityEngine;
using UnityEngine.AI;

public class WanderStrategy : IActionStrategy
{
    private readonly NavMeshAgent _agent;
    private readonly float _wanderRadius;

    public bool CanPerform => !Complete;
    public bool Complete => _agent.remainingDistance < 2f && !_agent.pathPending;

    public WanderStrategy(NavMeshAgent agent, float wanderRadius)
    {
        _agent = agent;
        _wanderRadius = wanderRadius;
    }

    public void Start()
    {
        for (var i = 0; i < 5; i++)
        {
            var randomDirection = Random.insideUnitSphere * _wanderRadius;
            randomDirection.y = 0;

            if (!NavMesh.SamplePosition(_agent.transform.position + randomDirection, out var hit, _wanderRadius,
                    1)) continue;
            
            _agent.SetDestination(hit.position);
        }
    }
}