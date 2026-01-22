using UnityEngine;

public class Bouncer : Entity
{
    [SerializeField] private PDir bounceDirection;
    [SerializeField] private float bounceSpeed;
    public override bool IsSolid => false;

    public override void OnCollide(DynamicEntity de, Vector2 normal)
    {
        // bool slammed = false;
        if (de is PlayerMovement pm)
        {
            pm.onGround?.Invoke();
            if (pm.SpecialState == SpecialState.Dash)
            {
                Dash.Instance.CancelDash();
            }
            if (pm.SpecialState == SpecialState.GroundSlam)
            {
                // slammed = true;
                // cancel slam
            }
        }
        bool isHorz = bounceDirection == PDir.Left || bounceDirection == PDir.Right;
        if (isHorz)
        {
            de.Velocity.x = -de.Velocity.x + Util.PDir2Vec(bounceDirection).x * bounceSpeed;
        }
        else
        {
            de.Velocity.y = Util.PDir2Vec(bounceDirection).y * bounceSpeed;
            if (bounceDirection == PDir.Up) de.OnAirborne();
        }
    }
}