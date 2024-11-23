using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GoapAgent : MonoBehaviour
{
    private readonly List<GoapSensor> _sensors = new();

    [SerializeField] private List<GameObject> dynamicLocations;

    public NavMeshAgent NavMeshAgent { get; private set; }

    private Rigidbody _rigidbody;

    [Header("Stats")] public float Health = 100f;
    public float Stamina = 100f;

    public List<string> Inventory = new();

    private CountdownTimer _statsTimer;

    private IGoapPlanner _planner;
    public ActionPlan ActionPlan { get; private set; }

    public Dictionary<string, AgentBelief> Beliefs { get; } = new();
    public HashSet<AgentAction> Actions { get; } = new();
    private AgentAction _currentAction;
    public HashSet<AgentGoal> Goals { get; private set; }
    public AgentGoal CurrentGoal { get; private set; }
    private AgentGoal _lastGoal;


    private void Awake()
    {
        NavMeshAgent = GetComponent<NavMeshAgent>();
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.freezeRotation = true;
        _planner = new GoapPlanner();
    }

    private void Start()
    {
        SetupBeliefs();
        SetupLocationsActionsAndBeliefs();
        SetupDynamicSensors();
        SetupTimers();
        SetupActions();
        SetupGoals();
        GameManager.Instance.AddAgent(this);
    }
    
    public static string GetBeliefNameForItem(string item)
    {
        return $"AgentHas{item}";
    }

    private void SetupDynamicItemBeliefs()
    {
        var beliefFactory = new BeliefFactory(this, Beliefs);
        GameManager.Instance.Items.ForEach(item =>
        {
            var itemName = item.itemName;
            var beliefName = GetBeliefNameForItem(itemName);
            beliefFactory.AddBelief(beliefName, () => Inventory.Contains(itemName));
        });
    }

    private void SetupLocationsActionsAndBeliefs()
    {
        var actionProviders = new List<IActionProvider>();
        dynamicLocations.ForEach(location =>
            actionProviders.AddRange(location.GetComponents<Component>().OfType<IActionProvider>()));

        foreach (var actionProvider in actionProviders)
        {
            actionProvider.GetBeliefs(this).ToList().ForEach(belief => Beliefs[belief.Key] = belief.Value);
        }

        foreach (var actionProvider in actionProviders)
        {
            Actions.UnionWith(actionProvider.GetActions(this, Beliefs));
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
        beliefFactory.AddBelief("AgentIdle", () => !NavMeshAgent.hasPath);
        beliefFactory.AddBelief("AgentMoving", () => NavMeshAgent.hasPath);
        beliefFactory.AddBelief("AgentHealthLow", () => Health < 30);
        beliefFactory.AddBelief("AgentIsHealthy", () => Health >= 50);
        beliefFactory.AddBelief("AgentStaminaLow", () => Stamina < 10);
        beliefFactory.AddBelief("AgentIsRested", () => Stamina >= 50);
        
        SetupDynamicItemBeliefs();
    }

    private void SetupActions()
    {
        Actions.Add(
            new AgentAction.Builder("Relax")
                .WithStrategy(new IdleStrategy(5))
                .AddEffect(Beliefs["Nothing"])
                .Build());
        Actions.Add(new AgentAction.Builder("Wander Around")
            .WithStrategy(new WanderStrategy(NavMeshAgent, 10))
            .AddEffect(Beliefs["AgentMoving"])
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
                .AddDesiredEffect(Beliefs["AgentHasRoomKey"])
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
        Stamina += Beliefs["AgentAtBedLocation"].Evaluate() ? 20 : -10;
        Stamina = Mathf.Clamp(Stamina, 0, 100);
        Health += Beliefs["AgentAtLobbyDeskLocation"].Evaluate() ? 10 : -5;
        Health = Mathf.Clamp(Health, 0, 100);
    }

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
                NavMeshAgent.ResetPath();

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