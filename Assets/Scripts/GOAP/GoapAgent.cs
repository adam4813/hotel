using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GoapAgent : MonoBehaviour
{
    [Header("Sensors")] [SerializeField] private GoapSensor chaseSensor;
    [SerializeField] private GoapSensor attackSensor;

    [Header("Known Locations")] [SerializeField]
    private Transform restingLocation;

    [SerializeField] private Transform doorLocation;
    [SerializeField] private Transform lobbyDeskLocation;

    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rigidbody;

    [Header("Stats")] public float Health = 100f;
    public float Stamina = 100f;

    private CountdownTimer _statsTimer;

    private GameObject _target;
    private Vector3 _destination;

    private AgentGoal _lastGoal;
    private AgentGoal _currentGoal;
    private ActionPlan _actionPlan;
    private AgentAction _currentAction;

    public HashSet<AgentAction> Actions { get; private set; }
    private Dictionary<string, AgentBelief> _beliefs;
    private HashSet<AgentGoal> _goals;

    private IGoapPlanner _planner;


    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.freezeRotation = true;
        
        _planner = new GoapPlanner();
    }

    private void Start()
    {
        SetupTimers();
        SetupBeliefs();
        SetupActions();
        SetupGoals();
    }

    private void SetupBeliefs()
    {
        _beliefs = new Dictionary<string, AgentBelief>();
        var beliefFactory = new BeliefFactory(this, _beliefs);

        beliefFactory.AddBelief("Nothing", () => false);
        beliefFactory.AddBelief("AgentIdle", () => !_navMeshAgent.hasPath);
        beliefFactory.AddBelief("AgentMoving", () => _navMeshAgent.hasPath);
        beliefFactory.AddBelief("AgentHealthLow", () => Health < 30);
        beliefFactory.AddBelief("AgentIsHealthy", () => Health >= 50);
        beliefFactory.AddBelief("AgentStaminaLow", () => Stamina < 10);
        beliefFactory.AddBelief("AgentIsRested", () => Stamina >= 50);
        
        beliefFactory.AddLocationBelief("AgentAtRestingLocation", 3f, restingLocation);
        beliefFactory.AddLocationBelief("AgentAtLobbyDeskLocation", 3f, lobbyDeskLocation);
        
        beliefFactory.AddSensorBelief("PlayerInChaseRange", chaseSensor);
        beliefFactory.AddSensorBelief("PlayerInAttackRange", attackSensor);
        
    }

    private void SetupActions()
    {
        Actions = new HashSet<AgentAction>
        {
            new AgentAction.Builder("Relax")
                .WithStrategy(new IdleStrategy(5))
                .AddEffect(_beliefs["Nothing"])
                .Build(),
            new AgentAction.Builder("Wander Around")
                .WithStrategy(new WanderStrategy(_navMeshAgent, 10))
                .AddEffect(_beliefs["AgentMoving"])
                .Build(),
            new AgentAction.Builder("MoveToRestingLocation")
                .WithStrategy(new MoveStrategy(_navMeshAgent, () => restingLocation.position))
                .AddEffect(_beliefs["AgentAtRestingLocation"])
                .Build(),
            new AgentAction.Builder("Rest")
                .WithStrategy(new IdleStrategy(4))
                .AddPrecondition(_beliefs["AgentAtRestingLocation"])
                .AddEffect(_beliefs["AgentIsRested"])
                .Build()
        };
    }

    private void SetupGoals()
    {
        _goals = new HashSet<AgentGoal>
        {
            new AgentGoal.Builder("ChillOut")
                .WithPriority(1)
                .AddDesiredEffect(_beliefs["Nothing"])
                .Build(),
            new AgentGoal.Builder("Wander")
                .WithPriority(1)
                .AddDesiredEffect(_beliefs["AgentMoving"])
                .Build(),
            new AgentGoal.Builder("Rest")
                .WithPriority(2)
                .AddDesiredEffect(_beliefs["AgentIsRested"])
                .Build()
        };
    }


    private void SetupTimers()
    {
        _statsTimer = new CountdownTimer(2f);
        _statsTimer.OnTimerStop += () =>
        {
            UpdateStats();
            _statsTimer.Start();
        };
        _statsTimer.Start();
    }

    private void UpdateStats()
    {
        Stamina += InRangeOf(restingLocation.position, 3f) ? 20 : -10;
        Stamina = Mathf.Clamp(Stamina, 0, 100);
        Health += InRangeOf(lobbyDeskLocation.position, 3f) ? 10 : -5;
        Health = Mathf.Clamp(Health, 0, 100);
    }

    private bool InRangeOf(Vector3 position, float distance) =>
        Vector3.Distance(transform.position, position) < distance;

    private void OnEnable()
    {
        chaseSensor.OnTargetChanged += HandleTargetChanged;
    }

    private void OnDisable()
    {
        chaseSensor.OnTargetChanged -= HandleTargetChanged;
    }

    private void HandleTargetChanged()
    {
        _currentAction = null;
        _currentGoal = null;
    }

    private void Update()
    {
        _statsTimer.Tick(Time.deltaTime);
        
        if (_currentAction == null)
        {
            CalculatePlan();
            
            if (_actionPlan != null && _actionPlan.Actions.Count > 0)
            {
                _navMeshAgent.ResetPath();
                
                _currentGoal = _actionPlan.Goal;
                _currentAction = _actionPlan.Actions.Pop();

                if (_currentAction.Preconditions.All(precondition => precondition.Evaluate()))
                {
                    _currentAction.Start();
                }
                else
                {
                    _currentAction = null;
                    _currentGoal = null;
                }
            }
        }

        if (_actionPlan == null || _currentAction == null) return;
        
        _currentAction.Update(Time.deltaTime);
            
        if (!_currentAction.Complete) return;
            
        _currentAction.Stop();
        _currentAction = null;

        if (_actionPlan.Actions.Count != 0) return;
            
        _lastGoal = _currentGoal;
        _currentGoal = null;
    }

    private void CalculatePlan()
    {
        var priorityLevel = _currentGoal?.Priority ?? 0;

        var goalsToCheck = _goals;

        if (_currentAction != null)
        {
            goalsToCheck = new HashSet<AgentGoal>(_goals.Where(goal => goal.Priority > priorityLevel));
        }
        
        var potentialPlan = _planner.Plan(this, goalsToCheck, _lastGoal);

        if (potentialPlan == null) return;
        
        _actionPlan = potentialPlan;
    }
}