using UnityEngine;

public class LobbyDesk : MonoBehaviour, IInteractable
{
    public string InteractionPrompt { get; }
    
    public bool OnInteract(Interactor interactor)
    {
        Debug.Log("Interacting with the lobby desk");
        return true;
    }
}
