public interface IInteractable
{
    public string InteractionPrompt { get; }
    public bool OnInteract(Interactor interactor);
}