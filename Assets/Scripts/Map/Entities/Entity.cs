using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [HideInInspector] public Collider2D Hitbox;

    public abstract bool IsSolid { get; }
    protected virtual void Awake()
    {
        Hitbox = GetComponent<Collider2D>();
    }
    

    // normal = player pointing to entity surface
    public abstract void OnCollide(DynamicEntity de, Vector2 normal);

}
