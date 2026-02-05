using UnityEngine;

public abstract class Trigger : MonoBehaviour
{
    private Collider2D col;

    protected virtual void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogWarning("Trigger missing collider component");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && collision.GetComponent<PlayerMovement>() != null)
        {
            OnPlayerEnter();
        } 
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && collision.GetComponent<PlayerMovement>() != null)
        {
            OnPlayerExit();
        }
    }

    public virtual void OnPlayerEnter()
    {

    }

    public virtual void OnPlayerExit()
    {

    }

    [ContextMenu("Zero BoxCollider Offset")]
    public void ZeroColliderOffset()
    {
        Collider2D col = GetComponent<Collider2D>();
        Transform t = col.transform;

        // World position of collider center BEFORE
        Vector3 worldCenterBefore = t.TransformPoint(col.offset);

        // Zero the offset
        col.offset = Vector2.zero;

        // World position of collider center AFTER
        Vector3 worldCenterAfter = t.TransformPoint(col.offset);

        // Move transform so collider stays in the same place
        Vector3 delta = worldCenterBefore - worldCenterAfter;
        t.position += delta;
    }


}