using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

// The external forces this object can currently experience
public enum BodyState
{
    OnGround, 
    InAir, // Experiences gravity and terminal drag
    Locked  // Cannot move and does not experience any force
}

// Any entity that can move. By default gravity and collisions are enabled. 
// Collision code courtesy of Ethan C for Mariposa
public class DynamicEntity : MonoBehaviour
{
    public BodyState State;

    public Collider2D SurfaceCollider;

    protected List<RaycastHit2D> collisionHits = new(); // All collision hits for the current frame

    protected float fdt; // Shorthand for fixed delta time

    public Vector2 Velocity;

    [Tooltip("The downward vertical acceleration applied when in the air")]
    public float Gravity;



    public bool GravityEnabled = true;
    public float GravityMultiplier = 1f;
    public bool CollisionsEnabled = true;



    [Tooltip("The maximum fall velocity")]
    public float TerminalVelocity;

    // Layers that this object receives forces from
    [SerializeField] protected LayerMask collisionLayer;


    public const float CONTACT_OFFSET = 0.005f; // The gap between this body and a surface after a collision
    protected const float COLLISION_CHECK_DISTANCE = 0.1f; // how far away you have to be from a ceiling or the ground to be considered "colliding" with it
    private const float MAX_SUBSTEPS = 5; // The maximum number of movement substeps ApplyMovement can take;

    protected virtual void Awake()
    {
        Unlock();
        ResolveInitialCollisions();
    }

    private void ResolveInitialCollisions()
    {
        Physics2D.SyncTransforms();
        Collider2D[] initialContacts = Physics2D.OverlapBoxAll(SurfaceCollider.bounds.center, SurfaceCollider.bounds.size, 0, collisionLayer);
        foreach (Collider2D collider in initialContacts)
        {
            ColliderDistance2D separation = SurfaceCollider.Distance(collider);
            transform.position += (separation.distance + CONTACT_OFFSET) * (Vector3)separation.normal;
        }
    }

    protected virtual void Update() { }
    protected virtual void FixedUpdate()
    {
        fdt = Time.deltaTime;

        CheckGrounded();
        Fall();

        Vector2 movement = Velocity * fdt;
        ApplyMovement(movement);
    }


    protected virtual void Fall()
    {
        if (State != BodyState.InAir || !GravityEnabled) return;
        Velocity.y = Mathf.Max(Velocity.y - Gravity * GravityMultiplier * fdt, -TerminalVelocity); // Cap the velocity
    }

    protected virtual void CheckGrounded()
    {
        if (!CollisionsEnabled) return;

        Bounds bounds = SurfaceCollider.bounds;
        Vector2 origin = (Vector2)transform.position + SurfaceCollider.offset + SurfaceCollider.bounds.extents.y * 3 / 4 * Vector2.down;
        Vector2 size = new(bounds.size.x * 0.99f, bounds.size.y / 4);
        Physics2D.SyncTransforms();
        RaycastHit2D groundHit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, Mathf.Infinity, collisionLayer);

        bool didHitGround = groundHit && groundHit.distance <= COLLISION_CHECK_DISTANCE;

        if (didHitGround) OnGrounded(groundHit);
        else if (!didHitGround) OnAirborne();
    }

    protected virtual void OnGrounded(RaycastHit2D groundHit)
    {
        if (State != BodyState.OnGround) State = BodyState.OnGround;
    }

    // 
    protected virtual void OnAirborne()
    {
        if (State == BodyState.OnGround) State = BodyState.InAir;
    }

    /// <summary>
    /// Applies a movement vector to this freebody and respects collision
    /// </summary>
    /// <param name="move">The movement vector</param>
    public virtual void ApplyMovement(Vector2 move)
    {
        if (!CollisionsEnabled) return;
        collisionHits.Clear();
        if (Mathf.Approximately(move.magnitude, 0f)) return; // This avoids weird imprecision errors

        Bounds bounds = SurfaceCollider.bounds;
        Vector2 origin = (Vector2)transform.position + SurfaceCollider.offset;
        RaycastHit2D hit = Physics2D.BoxCast(origin, bounds.size, 0f, move.normalized, move.magnitude, collisionLayer);

        // If the free body is inside another object, separate them and recompute the raycast
        if (hit && !hit.collider.isTrigger && Mathf.Approximately(hit.distance, 0f))
        {
            ResolveInitialCollisions();
            origin = (Vector2)transform.position + SurfaceCollider.offset;
            hit = Physics2D.BoxCast(origin, bounds.size, 0f, move.normalized, move.magnitude, collisionLayer);
        }

        int substeps = 0; // This is to prevent infinite loops in case something goes wrong
        while (hit && !hit.collider.isTrigger)
        {

            collisionHits.Add(hit);
            Vector2 normal = hit.normal.normalized;
            Vector2 delta = hit.centroid - origin + CONTACT_OFFSET * normal;
            move -= delta;

            transform.position += (Vector3)delta;
            origin = (Vector2)transform.position + SurfaceCollider.offset;
            move -= Util.Vec2Proj(move, normal);
            Velocity -= Util.Vec2Proj(Velocity, normal);

            // These conditionals sidestep floating point imprecisions
            if (Mathf.Approximately(Velocity.sqrMagnitude, 0f))
            {
                Velocity = Vector2.zero;
                break;
            }
            if (Mathf.Approximately(move.sqrMagnitude, 0f))
            {
                move = Vector2.zero;
                break;
            }

            hit = Physics2D.BoxCast(origin, bounds.size, 0f, move.normalized, move.magnitude, collisionLayer);

            substeps++;
            if (substeps > MAX_SUBSTEPS) break;
        }

        // Apply lingering movement
        transform.position += (Vector3)move;
    }

    // Drop entities in the air at the start of a scene or after an interaction
    public virtual void Unlock()
    {
        State = BodyState.InAir;
    }

    public void Stop()
    {
        Velocity = Vector3.zero;
    }
}
