using UnityEngine;

public class Zipline : Entity, IInteractable
{
    [SerializeField] Transform node;
    [SerializeField] InteractionTrigger ziplineTrigger;

    public override bool IsSolid => false;

    protected override void Awake()
    {
        base.Awake();
        // draw line to node
    }

    public InteractionTrigger GenerateInteractionTrigger()
    {
        InteractionTrigger intTrigger = GameObject.Instantiate(ziplineTrigger, transform);
        // reshape
        intTrigger.SetInteractable(this);
        return intTrigger;
    }

    public bool CanInteract()
    {
        return true;
    }

    public void OnInteract()
    {
        Debug.Log("Riding zipline");
        // calculate position to attach
        // teleport player onto line and set special state, disable gravity and friction?
    }
}