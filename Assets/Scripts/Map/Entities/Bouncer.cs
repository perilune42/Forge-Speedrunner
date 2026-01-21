using UnityEngine;

public class Bouncer : Entity
{
    public override bool IsSolid => false;

    public override void OnCollide(DynamicEntity de, Vector2 normal)
    {
        Debug.Log("Yeowch");
    }
}