using UnityEngine;

public interface IInteractable
{
    public Transform transform { get; }
    public InteractionTrigger GenerateInteractionTrigger()
    {
        InteractionTrigger intTrigger = GameObject.Instantiate(RoomManager.Instance.InteractionTriggerPrefab, transform);
        intTrigger.SetInteractable(this);
        return intTrigger;
    }
    
    public void OnEnterInteractRange() { }
    public void OnExitInteractRange() { }

    public void OnInteract();

    public bool CanInteract();
}