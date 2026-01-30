using UnityEngine;

public class DummyInteractable : Entity, IInteractable
{
    public override bool IsSolid => false;

    public bool CanInteract() => true;

    public void OnInteract()
    {
        Debug.Log("Hello!");
    }

    public void OnEnterInteractRange()
    {
        GetComponent<SpriteRenderer>().color = Color.yellow;
    }

    public void OnExitInteractRange()
    {
        GetComponent<SpriteRenderer>().color = Color.white;
    }
}