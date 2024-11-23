using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(QueueableLine))]
public class LobbyDesk : MonoBehaviour, IInteractable, IActionProvider
{
    public string InteractionPrompt { get; }
    private QueueableLine _queueableLine;

    private void Awake()
    {
        _queueableLine = GetComponent<QueueableLine>();
    }

    public bool OnInteract(Interactor interactor)
    {
        var guestInQueue = _queueableLine.GetNextInQueue();
        if (guestInQueue != null)
        {
            Debug.Log($"Interacting with the {guestInQueue.name} in the queue");
            if (guestInQueue.TryGetComponent(out GoapAgent agent))
            {
                agent.AddItemToInventory("RoomKey");
            }
        }
        else
        {
            Debug.Log("No one in the queue");
        }

        return true;
    }


    public HashSet<AgentAction> GetActions(GoapAgent agent, Dictionary<string, AgentBelief> beliefs)
    {
        var hasRoomKeyBelief = beliefs[GoapAgent.GetBeliefNameForItem("RoomKey")];
        var queueBeliefName = GetComponent<QueueableLine>().QueueBeliefName;
        return new HashSet<AgentAction>
        {
            new AgentAction.Builder($"WaitToBeCheckedInAt{name}")
                .WithStrategy(new WaitForBeliefStrategy(hasRoomKeyBelief))
                .AddPrecondition(beliefs[queueBeliefName])
                .AddEffect(hasRoomKeyBelief)
                .Build()
        };
    }
}