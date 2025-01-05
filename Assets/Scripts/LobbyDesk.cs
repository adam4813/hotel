using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(QueueableLine))]
public class LobbyDesk : MonoBehaviour, IInteractable, IActionProvider
{
    [SerializeField] private LobbyDeskPanel lobbyDeskPanel;
    public string InteractionPrompt { get; }
    private QueueableLine _queueableLine;

    private void Awake()
    {
        _queueableLine = GetComponent<QueueableLine>();
    }

    private void OnEnable()
    {
        LobbyDeskPanel.OnGuestCheckedIn += OnGuestCheckedIn;
    }

    private void OnDisable()
    {
        LobbyDeskPanel.OnGuestCheckedIn -= OnGuestCheckedIn;
    }

    private void OnGuestCheckedIn(GoapAgent guest)
    {
        if (!_queueableLine.IsInQueue(guest.gameObject)) return;
        _queueableLine.RemoveFromQueue(guest.gameObject);
        guest.AddItemToInventory("RoomKey");
    }

    public bool OnInteract(Interactor interactor)
    {
        if (_queueableLine.IsQueueEmpty)
        {
            Debug.Log("No one in the queue");
            return false;
        }

        _queueableLine.GetQueue().ForEach(guest =>
        {
            if (!guest.TryGetComponent(out GoapAgent agent)) return;
            lobbyDeskPanel.AddGuest(agent);
        });

        lobbyDeskPanel.gameObject.SetActive(true);

        return true;
    }

    public HashSet<AgentAction> GetActions(GoapAgent agent, Dictionary<string, AgentBelief> beliefs)
    {
        var hasRoomKeyBelief = beliefs[GoapAgent.GetBeliefNameForItem("RoomKey")];
        var queueBeliefName = GetComponent<QueueableLine>().QueueBeliefName;
        return new HashSet<AgentAction>
        {
            new AgentAction.Builder($"WaitToBeCheckedInAt{name}Desk{GetInstanceID()}")
                .WithStrategy(new WaitForBeliefStrategy(hasRoomKeyBelief))
                .AddPrecondition(beliefs[queueBeliefName])
                .AddEffect(hasRoomKeyBelief)
                .Build()
        };
    }
}