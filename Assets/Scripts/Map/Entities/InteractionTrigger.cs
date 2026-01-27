using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    bool playerInside;
    IInteractable interactable;

    public void SetInteractable(IInteractable i)
    {
        this.interactable = i;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>() != null)
        {
            playerInside = true;
        }
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