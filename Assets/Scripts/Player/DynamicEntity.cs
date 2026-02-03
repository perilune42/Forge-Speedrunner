using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

// The external forces this object can currently experience
public enum BodyState
{
    OnGround, 
    InAir, // Experiences gravity and terminal drag
    Override    // Can still move by velocity but does not experience any force

}

public enum PDir
{
    Left, Right, Up, Down
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
    public Stat GravityMultiplier = new Stat(1f);
    public bool CollisionsEnabled = true;
    public bool Locked = false;

    public Action OnHitWallLeft, OnHitWallRight;
    public Action<Entity, Vector2> OnHitWallAny;

    [Tooltip("The maximum fall velocity")]
    public float TerminalVelocity;

    // Layers that this object receives forces from
    [SerializeField] protected LayerMask collisionLayer;
    // Layers that this object interacts with via collision but not necessarily forces
    [SerializeField] protected LayerMask interactLayer;

    private List<Entity> collidingEntities = new();


    public const float CONTACT_OFFSET = 0.005f; // The gap between this body and a surface after a collision
    protected const float COLLISION_CHECK_DISTANCE = 0.1f; // how far away you have to be from a ceiling or the ground to be considered "colliding" with it
    private const float MAX_SUBSTEPS = 5; // The maximum number of movement substeps ApplyMovement can take;

    private bool canHitCeiling;
    protected virtual void Awake()
    {
        Unlock();
        ResolveInitialCollisions();
    }

    private void ResolveInitialCollisions(bool collision = true)
    {
        Physics2D.SyncTransforms();
        var targetLayer = collision ? collisionLayer : interactLayer;
        Collider2D[] initialContacts = Physics2D.OverlapBoxAll(SurfaceCollider.bounds.center, SurfaceCollider.bounds.size, 0, targetLayer);
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
        if (Locked) return;

        Fall();
        Vector2 movement = Velocity * fdt;
        ApplyMovement(movement);
        CheckGrounded();
    }


    protected virtual void Fall()
    {
        if (State == BodyState.Override) return;
        if (State != BodyState.InAir || !GravityEnabled) return;
        Velocity.y = Mathf.Max(Velocity.y - Gravity * GravityMultiplier.Get() * fdt, -TerminalVelocity); // Cap the velocity
    }

    protected virtual void CheckGrounded()
    {
        if (!CollisionsEnabled || State == BodyState.Override) return;

        Bounds bounds = SurfaceCollider.bounds;
        Vector2 origin = (Vector2)transform.position + SurfaceCollider.offset + SurfaceCollider.bounds.extents.y * 3 / 4 * Vector2.down;
        Vector2 size = new(bounds.size.x * 0.99f, bounds.size.y / 4);
        Physics2D.SyncTransforms();
        RaycastHit2D groundHit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, Mathf.Infinity, collisionLayer);

        bool didHitGround = groundHit && groundHit.distance <= COLLISION_CHECK_DISTANCE;
        if (didHitGround && (Velocity.y <= 1e-2 || Mathf.Approximately(Velocity.y, 0)) && State != BodyState.OnGround) OnGrounded(groundHit);
        else if (!didHitGround && State == BodyState.OnGround) OnAirborne();

        if (Velocity.y < 0 || State == BodyState.OnGround) canHitCeiling = true;
        if (canHitCeiling)
        {
            Vector2 originTop = (Vector2)transform.position + SurfaceCollider.offset + SurfaceCollider.bounds.extents.y * 3 / 4 * Vector2.up;
            RaycastHit2D groundHitTop = Physics2D.BoxCast(originTop, size, 0f, Vector2.up, Mathf.Infinity, collisionLayer);
            bool didHitGroundTop = groundHitTop && groundHitTop.distance <= COLLISION_CHECK_DISTANCE;
          
            if (didHitGroundTop && (Velocity.y >= -1e-2 ) && State != BodyState.OnGround) OnGroundedTop(groundHitTop);
        }
        
    }

    

    public virtual void OnGrounded(RaycastHit2D groundHit)
    {
        if (State != BodyState.OnGround) State = BodyState.OnGround;
    }

    /// <summary>
    /// For when this entity (usually the player) bonks its head on top of a collider
    /// </summary>
    public virtual void OnGroundedTop(RaycastHit2D groundHit)
    {
        canHitCeiling = false;
    }
    // 
    public virtual void OnAirborne()
    {
        if (State == BodyState.OnGround) State = BodyState.InAir;
    }

    /// <summary>
    /// Applies a movement vector to this freebody and respects collision
    /// </summary>
    /// <param name="move">The movement vector</param>
    public virtual void ApplyMovement(Vector2 move)
    {
        bool doesCollide = CollisionsEnabled && (State != BodyState.Override);

        collisionHits.Clear();
        if (Mathf.Approximately(move.magnitude, 0f)) return; // This avoids weird imprecision errors

        Bounds bounds = SurfaceCollider.bounds;
        Vector2 origin = (Vector2)transform.position + SurfaceCollider.offset;

        LayerMask collideOrInteractLayer = collisionLayer.value | interactLayer.value;

        { 
            // prioritize entities over solids to avoid clipping

            RaycastHit2D hit;

            hit = Physics2D.BoxCast(origin, bounds.size, 0f, move.normalized, move.magnitude, interactLayer);
            if (hit && !hit.collider.isTrigger && Mathf.Approximately(hit.distance, 0f))
            {
                Entity hitEntity = hit.collider.GetComponent<Entity>();
                if (hitEntity != null && hitEntity.StrictCollisions)
                {
                    ResolveInitialCollisions(false);
                    origin = (Vector2)transform.position + SurfaceCollider.offset;
                }
            }
            if (doesCollide)
            {
                hit = Physics2D.BoxCast(origin, bounds.size, 0f, move.normalized, move.magnitude, collisionLayer);
                if (hit && !hit.collider.isTrigger && Mathf.Approximately(hit.distance, 0f))
                {
                    ResolveInitialCollisions();
                    origin = (Vector2)transform.position + SurfaceCollider.offset;
                }
            }

        }
        // hit = Physics2D.BoxCast(origin, bounds.size, 0f, move.normalized, move.magnitude, collideOrInteractLayer);

        bool hitL = false, hitR = false;
        Dictionary<Entity, bool> stillTouchingEntity = new Dictionary<Entity, bool>();
        foreach (Entity entity in collidingEntities)
        {
            stillTouchingEntity[entity] = false;
        }

        List<Tuple<Entity, Vector2>> toCollide = new();
        int substeps = 0; // This is to prevent infinite loops in case something goes wrong

        RaycastHit2D[] hits;
        hits = Physics2D.BoxCastAll(origin, bounds.size, 0f, move.normalized, move.magnitude, collideOrInteractLayer);


        while (hits.Length > 0)
        {
            bool moved = false;
            foreach (var hit in hits)
            {
                if (!hit) break;

                collisionHits.Add(hit);
                Vector2 normal = hit.normal.normalized;


                bool hitSolid = true;
                Entity hitEntity = hit.collider.GetComponent<Entity>();

                if (hitEntity != null)
                {
                    if (!collidingEntities.Contains(hitEntity))
                    {
                        
                        collidingEntities.Add(hitEntity);
                        var prevMove = move;
                        hitEntity.OnCollide(this, hit.normal);
                        move = Velocity * fdt;  // velocity may have been altered by entity interaction
                        if (!Mathf.Approximately((prevMove - move).sqrMagnitude, 0))
                        {
                            moved = true;
                        }
                    }
                    stillTouchingEntity[hitEntity] = true;
                    if (!hitEntity.IsSolid)
                    {
                        hitSolid = false;
                    }
                }
                else if (((1 << hit.collider.gameObject.layer) & collisionLayer.value) == 0)
                {
                    hitSolid = false;
                }

                if (hitSolid && doesCollide)
                {
                    Vector2 delta = hit.centroid - origin + CONTACT_OFFSET * normal;
                    move -= delta;
                    if (!Mathf.Approximately(delta.sqrMagnitude, 0))
                    {
                        moved = true;
                    }

                    transform.position += (Vector3)delta;
                    origin = (Vector2)transform.position + SurfaceCollider.offset;
                    move -= Util.Vec2Proj(move, normal);
                    Velocity -= Util.Vec2Proj(Velocity, normal);

                    OnHitWallAny?.Invoke(hitEntity, -normal);

                    if (!hitL && normal.x > 0)
                    {
                        OnHitWallLeft?.Invoke();
                        hitL = true;
                    }
                    else if (!hitR && normal.x < 0)
                    {
                        OnHitWallRight?.Invoke();
                        hitR = true;
                    }

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
                }

                if (moved) break;


                substeps++;
                if (substeps > MAX_SUBSTEPS)
                {
                    Debug.LogWarning("Collision max substeps exceeded");
                    break;
                }
            }

            if (!moved)
            {
                // scanned everything here and didn't move, so probably not touching anything
                break;
            }

            hits = Physics2D.BoxCastAll(origin, bounds.size, 0f, move.normalized, move.magnitude, collideOrInteractLayer);
            substeps++;
            if (substeps > MAX_SUBSTEPS)
            {
                Debug.LogWarning("Collision max substeps exceeded");
                break;
            }
        }
        // resolve entity collisions now
        //foreach (var colData in toCollide)
        //{
        //    colData.Item1.OnCollide(this, colData.Item2);
        //}

        // prune entities that we have stopped colliding with
        for (int i = collidingEntities.Count - 1; i >= 0; i--) 
        {
            Entity e = collidingEntities[i];
            if (!stillTouchingEntity[e])
            {
                collidingEntities.RemoveAt(i);
            }
        }

        // Apply lingering movement
        transform.position += (Vector3)move;



    }

    public float GetSurfaceDistance(Vector2 dir, float maxDist = 10f)
    {
        var hit = Physics2D.BoxCast((Vector2)transform.position + SurfaceCollider.offset, SurfaceCollider.bounds.size, 0f, dir, maxDist, collisionLayer);
        return hit.distance;
    }

    public bool IsTouching(Vector2 dir)
    {
        var hit = Physics2D.BoxCast((Vector2)transform.position + SurfaceCollider.offset, SurfaceCollider.bounds.size, 0f, dir, 1f, collisionLayer);
        return hit.distance <= COLLISION_CHECK_DISTANCE;
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
