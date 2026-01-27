using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [HideInInspector] public Collider2D Hitbox;

    public abstract bool IsSolid { get; }
    public virtual bool StrictCollisions => false;
    protected virtual void Awake()
    {
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
        if (this is IInteractable)
        {

        }
    }
    

    // normal = player pointing to entity surface
    public virtual void OnCollide(DynamicEntity de, Vector2 normal) { }

}
