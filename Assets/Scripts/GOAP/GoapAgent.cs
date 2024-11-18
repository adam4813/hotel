using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GoapAgent : MonoBehaviour
{
    [Serializable]
    private struct NamedLocation
    {
        public string Name;
        public float Radius;
        public Transform Location;
    }

    private readonly List<GoapSensor> _sensors = new();

    [Header("Known Locations")] [SerializeField]
    private Transform doorLocation;

    [SerializeField] private Transform lobbyDeskLocation;
    [SerializeField] private List<NamedLocation> dynamicLocations;

    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rigidbody;

    [Header("Stats")] public float Health = 100f;
    public float Stamina = 100f;
    public List<string> Inventory = new();

    private CountdownTimer _statsTimer;

    private GameObject _target;
    private Vector3 _destination;

    private AgentGoal _lastGoal;
    public ActionPlan ActionPlan { get; private set; }

    public HashSet<AgentAction> Actions { get; } = new();
    private AgentAction _currentAction;

    public Dictionary<string, AgentBelief> Beliefs { get; } = new();

    public HashSet<AgentGoal> Goals { get; private set; }
    public AgentGoal CurrentGoal { get; private set; }

    private IGoapPlanner _planner;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.freezeRotation = true;
        _planner = new GoapPlanner();
        GameManager.Instance.AddAgent(this);
    }

    private void Start()
    {
        SetupDynamicLocations();
        SetupDynamicSensors();
        SetupTimers();
        SetupBeliefs();
        SetupActions();
        SetupGoals();
    }

    private void SetupDynamicLocations()
    {
        foreach (var location in dynamicLocations)
        {
            var beliefName = $"AgentAt{location.Name}";
            var beliefFactory = new BeliefFactory(this, Beliefs);
            beliefFactory.AddLocationBelief(beliefName, location.Radius, location.Location);

            Actions.Add(new AgentAction.Builder($"MoveTo{location.Name}")
                .WithStrategy(new MoveStrategy(_navMeshAgent, () => location.Location.position))
                .AddEffect(Beliefs[beliefName])
                .Build());
        }
    }

    private void SetupDynamicSensors()
    {
        foreach (var sensor in GetComponentsInChildren<GoapSensor>().ToList())
        {
            _sensors.Add(sensor);
            var beliefName = $"ObjectIn{sensor.gameObject.name}Range";
            var beliefFactory = new BeliefFactory(this, Beliefs);
            beliefFactory.AddSensorBelief(beliefName, sensor);
        }
    }

    private void SetupBeliefs()
    {
        var beliefFactory = new BeliefFactory(this, Beliefs);

        beliefFactory.AddBelief("Nothing", () => false);
        beliefFactory.AddBelief("AgentIdle", () => !_navMeshAgent.hasPath);
        beliefFactory.AddBelief("AgentMoving", () => _navMeshAgent.hasPath);
        beliefFactory.AddBelief("AgentHealthLow", () => Health < 30);
        beliefFactory.AddBelief("AgentIsHealthy", () => Health >= 50);
        beliefFactory.AddBelief("AgentStaminaLow", () => Stamina < 10);
        beliefFactory.AddBelief("AgentIsRested", () => Stamina >= 50);
        beliefFactory.AddBelief("AgentInLobbyQueue",
            () => lobbyDeskLocation.gameObject.GetComponent<QueueableLine>().IsInQueue(gameObject));
        beliefFactory.AddBelief("AgentIsCheckedIn", () => Inventory.Contains("RoomKey"));
    }

    private void SetupActions()
    {
        Actions.Add(
            new AgentAction.Builder("Relax")
                .WithStrategy(new IdleStrategy(5))
                .AddEffect(Beliefs["Nothing"])
                .Build());
        Actions.Add(new AgentAction.Builder("Wander Around")
            .WithStrategy(new WanderStrategy(_navMeshAgent, 10))
            .AddEffect(Beliefs["AgentMoving"])
            .Build());
        Actions.Add(new AgentAction.Builder("Rest")
            .WithStrategy(new IdleStrategy(4))
            .AddPrecondition(Beliefs["AgentAtRestingLocation"])
            .AddEffect(Beliefs["AgentIsRested"])
            .Build());
        Actions.Add(new AgentAction.Builder("QueueInLobbyLine")
            .WithStrategy(new QueueInLineStrategy(lobbyDeskLocation.gameObject.GetComponent<QueueableLine>(),
                gameObject))
            .AddPrecondition(Beliefs["AgentAtLobbyDeskLocation"])
            .AddEffect(Beliefs["AgentInLobbyQueue"])
            .Build());
        Actions.Add(new AgentAction.Builder("WaitToBeCheckedIn")
            .WithStrategy(new IdleStrategy(10))
            .AddPrecondition(Beliefs["AgentInLobbyQueue"])
            .AddEffect(Beliefs["AgentIsCheckedIn"])
            .Build());
    }

    private void SetupGoals()
    {
        Goals = new HashSet<AgentGoal>
        {
            new AgentGoal.Builder("ChillOut")
                .WithPriority(1)
                .AddDesiredEffect(Beliefs["Nothing"])
                .Build(),
            new AgentGoal.Builder("Wander")
                .WithPriority(1)
                .AddDesiredEffect(Beliefs["AgentMoving"])
                .Build(),
            new AgentGoal.Builder("Rest")
                .WithPriority(2)
                .AddDesiredEffect(Beliefs["AgentIsRested"])
                .Build(),
            new AgentGoal.Builder("CheckedIn")
                .WithPriority(5)
                .AddDesiredEffect(Beliefs["AgentInLobbyQueue"])
                .AddDesiredEffect(Beliefs["AgentIsCheckedIn"])
                .OneShot()
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
        Stamina += Beliefs["AgentAtRestingLocation"].Evaluate() ? 20 : -10;
        Stamina = Mathf.Clamp(Stamina, 0, 100);
        Health += Beliefs["AgentAtLobbyDeskLocation"].Evaluate() ? 10 : -5;
        Health = Mathf.Clamp(Health, 0, 100);
    }

    private bool InRangeOf(Vector3 position, float distance) =>
        Vector3.Distance(transform.position, position) < distance;

    private void OnEnable()
    {
        _sensors.ForEach(sensor =>
        {
            if (sensor.IsChaseSensor)
            {
                sensor.OnTargetChanged += HandleTargetChanged;
            }
        });
    }

    private void OnDisable()
    {
        _sensors.ForEach(sensor =>
        {
            if (sensor.IsChaseSensor)
            {
                sensor.OnTargetChanged -= HandleTargetChanged;
            }
        });
    }

    private void HandleTargetChanged()
    {
        _currentAction = null;
        CurrentGoal = null;
    }

    private void Update()
    {
        _statsTimer.Tick(Time.deltaTime);

        if (_currentAction == null)
        {
            CalculatePlan();

            if (ActionPlan != null && ActionPlan.Actions.Count > 0)
            {
                _navMeshAgent.ResetPath();

                CurrentGoal = ActionPlan.Goal;
                _currentAction = ActionPlan.Actions.Pop();

                if (_currentAction.Preconditions.All(precondition => precondition.Evaluate()))
                {
                    _currentAction.Start();
                }
                else
                {
                    _currentAction = null;
                    CurrentGoal = null;
                }
            }
        }

        if (ActionPlan == null || _currentAction == null) return;

        _currentAction.Update(Time.deltaTime);

        if (!_currentAction.Complete) return;

        _currentAction.Stop();
        _currentAction = null;

        if (ActionPlan.Actions.Count != 0) return;

        _lastGoal = CurrentGoal;
        if (CurrentGoal is { IsOneShot: true })
        {
            Goals.Remove(CurrentGoal);
        }

        CurrentGoal = null;
    }

    private void CalculatePlan()
    {
        var priorityLevel = CurrentGoal?.Priority ?? 0;

        var goalsToCheck = Goals;

        if (_currentAction != null)
        {
            goalsToCheck = new HashSet<AgentGoal>(Goals.Where(goal => goal.Priority > priorityLevel));
        }

        var potentialPlan = _planner.Plan(this, goalsToCheck, _lastGoal);

        if (potentialPlan == null) return;

        ActionPlan = potentialPlan;
    }

    public void AddItemToInventory(string itemName)
    {
        Inventory.Add(itemName);
    }
}