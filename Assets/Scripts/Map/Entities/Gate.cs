using UnityEngine;

public class Gate : ActivatableEntity
{
    public Transform EndPoint;
    public float MoveSpeed;
    private bool isActivated;
    private bool isMoving;

    private Vector2 startPos;
    private Vector2 endPos;
    public override bool IsSolid => true;

    protected override void Awake()
    {
        base.Awake();
        endPos = transform.localPosition + EndPoint.localPosition; // child will move in world space with parent
        startPos = transform.localPosition;

    }
    public override void OnActivate()
    {
        if (isActivated) return;
        isActivated = true;
        isMoving = true;

    }
    // the reverse of OnActivate
    public override void ResetEntity()
    {
        isActivated = false; 
        isMoving = false;
        this.transform.localPosition = startPos;
    }

    protected override void FixedUpdate()
    {
        if (isMoving)
        {
            // fix end node in world space
            float dist = MoveSpeed * Time.fixedDeltaTime;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, endPos, dist);
            if (Vector2.Distance(transform.position, endPos) < dist)
            {
                isMoving = false;
                transform.localPosition = endPos;
            }
        }
    }


}
