using System.Collections.Generic;
using UnityEngine;

public class Wire : Entity
{
    public override bool IsSolid => false;
    public override bool StrictCollisions => true;

    [SerializeField] Animator animator;

    public override void OnCollide(DynamicEntity de, Vector2 normal)
    {
        base.OnCollide(de, normal);

        animator.Play("WireBumped");
    }
}
