using UnityEditor;
using UnityEngine;


public abstract class Entity : MonoBehaviour
{
    [HideInInspector] public Collider2D Hitbox;

    public abstract bool IsSolid { get; }
    public virtual bool StrictCollisions => false;

    protected bool playerInside;

    protected virtual void Awake()
    {
        if (this is IInteractable interactable) 
        {
            interactable.GenerateInteractionTrigger();
        }

        Hitbox = GetComponent<Collider2D>();
        if (IsSolid && (1 << gameObject.layer) != LayerMask.GetMask("Solid"))
        {
            Debug.LogWarning($"Solid entity set to wrong layer ({gameObject.layer})!");
            gameObject.layer = LayerMask.NameToLayer("Solid");
        }
        else if (!IsSolid && (1 << gameObject.layer) != LayerMask.GetMask("Entity"))
        {
            Debug.LogWarning($"Non-solid entity set to wrong layer ({gameObject.layer})!");
            gameObject.layer = LayerMask.NameToLayer("Entity");
        }
    }

    protected virtual void FixedUpdate()
    {

    }
    

    // normal = player pointing to entity surface
    public virtual void OnCollide(DynamicEntity de, Vector2 normal) { }

    // entity hitbox can also be used a trigger in a pinch
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && collision.GetComponent<PlayerMovement>() != null)
        {
            OnPlayerEnter();
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && collision.GetComponent<PlayerMovement>() != null)
        {
            OnPlayerExit();
        }
    }

    public virtual void OnPlayerEnter()
    {
        playerInside = true;
    }

    public virtual void OnPlayerExit()
    {
        playerInside = false;
    }


    public virtual void OnValidate()
    {
        // Debug.Log($"OnValidate called on {this}");
    }

}


