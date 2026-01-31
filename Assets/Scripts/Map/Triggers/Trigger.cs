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
}