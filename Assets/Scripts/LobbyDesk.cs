using UnityEngine;

[RequireComponent(typeof(QueueableLine))] 
public class LobbyDesk : MonoBehaviour, IInteractable
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
}