using UnityEngine;

public abstract class Trigger : MonoBehaviour
{
    private BoxCollider2D col;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        if (col == null)
        {
            Debug.LogWarning("Trigger missing collider component");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            OnPlayerEnter();
        } 
    }

    public virtual void OnPlayerEnter()
    {

    }
}