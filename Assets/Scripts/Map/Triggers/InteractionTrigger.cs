using UnityEngine;

public class InteractionTrigger : Trigger
{
    bool playerInside;
    IInteractable interactable;

    public void SetInteractable(IInteractable i)
    {
        this.interactable = i;
    }

    public override void OnPlayerEnter()
    {
        base.OnPlayerEnter();
        interactable.OnEnterInteractRange();
        playerInside = true;
    }

    public override void OnPlayerExit()
    {
        base.OnPlayerExit();
        interactable.OnExitInteractRange();
        playerInside = false;
    }


    private void FixedUpdate()
    {
        if (playerInside && PInput.Instance.Interact.HasPressed)
        {
            if (interactable.CanInteract())
            {
                interactable.OnInteract();
                PInput.Instance.Interact.ConsumeBuffer();
            }
        }
    }


}