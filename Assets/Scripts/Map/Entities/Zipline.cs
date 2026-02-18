using Unity.VisualScripting;
using UnityEngine;

public class Zipline : Entity, IInteractable
{
    [SerializeField] Transform node;
    [SerializeField] InteractionTrigger ziplineTrigger;

    public override bool IsSolid => false;

    const float interactRadius = 2f;
    const float ziplineSpeed = 20f;

    private bool playerIsRiding = false;
    private bool towardsNode = false;
    Vector2 vecToNode;
    PlayerMovement pm => Player.Instance.Movement;
    [SerializeField] LineRenderer lineRenderer;

    Vector2 positionOffset;

    protected override void Awake()
    {
        base.Awake();

        vecToNode = (node.transform.position - transform.position).normalized;
        positionOffset = new Vector2(0, -Player.Instance.Movement.PlayerHeight - 0.2f);
        pm.OnSpecialStateChange += (SpecialState state) =>
        {
            if (state != SpecialState.Normal && state != SpecialState.Zipline) CancelZipline();
        };
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (playerIsRiding)
        {
            StayOnZipline();
        }
        if (playerIsRiding && PInput.Instance.Jump.HasPressed)
        {
            JumpOffZipline();
            PInput.Instance.Jump.ConsumeBuffer();
        }
    }

    [ContextMenu("Set Line Positions")]
    private void SetLinePositions()
    {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, node.position);
    }

    public override void OnValidate()
    {
        SetLinePositions();
    }

    public InteractionTrigger GenerateInteractionTrigger()
    {
        InteractionTrigger intTrigger = GameObject.Instantiate(ziplineTrigger, transform);
        var capsule = intTrigger.GetComponent<CapsuleCollider2D>();
        Util.StretchCapsuleBetween(capsule, transform.position, node.position, interactRadius);
        intTrigger.SetInteractable(this);
        return intTrigger;
    }

    public bool CanInteract()
    {
        return !playerIsRiding;
    }

    public void OnInteract()
    {
        RideZipline();
        // calculate position to attach
        // teleport player onto line and set special state, disable gravity and friction?
    }

    private void RideZipline()
    {
        pm.State = BodyState.Override;
        pm.SpecialState = SpecialState.Zipline;
        pm.transform.position = Util.ProjectPointOntoSegment(pm.transform.position, transform.position, node.position) + positionOffset;
        Vector2 vecToNode = (node.transform.position - transform.position).normalized;
        towardsNode = Vector2.Dot(pm.FacingDir, vecToNode) > 0;
        if (vecToNode.y > 0.9) towardsNode = true;
        else if (vecToNode.y < -0.9) towardsNode = false;
        if (transform.position.x == node.transform.position.x) towardsNode = true;
        pm.Velocity = towardsNode ? vecToNode * ziplineSpeed : -vecToNode * ziplineSpeed;
        playerIsRiding = true;

        int projRegion = Util.ProjectionRegion((Vector2)pm.transform.position - positionOffset, transform.position, node.position);
        if (projRegion == -1)
        {
            pm.transform.position = (Vector2)transform.position + positionOffset + vecToNode * 0.05f;
        }
        else if (projRegion == 1) 
        {
            pm.transform.position = (Vector2)node.transform.position + positionOffset - vecToNode * 0.05f;
        }
        pm.onGround?.Invoke();
    }

    private void StayOnZipline()
    {
        pm.Velocity = towardsNode ? vecToNode * ziplineSpeed : -vecToNode * ziplineSpeed;
        int projRegion = Util.ProjectionRegion((Vector2)pm.transform.position - positionOffset, transform.position, node.position);
        // -1 = root side, 0 = on line, 1 = node side
        if (projRegion != 0)
        {
            JumpOffZipline();
        }
    }

    private void JumpOffZipline()
    {
        playerIsRiding = false;
        pm.State = BodyState.InAir;
        pm.SpecialState = SpecialState.Normal;
        pm.Jump();
    }

    private void CancelZipline()
    {
        playerIsRiding = false;
        pm.State = BodyState.InAir;
    }


}
