using UnityEngine;

public class PlayerWalkParticles : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private float baseStrength;

    private void Update()
    {
        float strength = 1f;
        if (playerMovement.State != BodyState.OnGround || playerMovement.SpecialState != SpecialState.Normal) strength = 0;
        else if (playerMovement.MoveDir.x == 0) strength = 0;
        var emission = particle.emission;
        var minMaxCurve = emission.rateOverDistance;
        minMaxCurve.constantMin = strength * baseStrength;
        minMaxCurve.constantMax = strength * baseStrength * 2f;
        emission.rateOverDistance = minMaxCurve;
        var shape = particle.shape;
        shape.scale = new Vector3(playerMovement.MoveDir.x, 1, 1);

    }
}
