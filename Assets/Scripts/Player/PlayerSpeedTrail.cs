using System;
using UnityEngine;

public class PlayerSpeedTrail : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private float minSpeedThresold, baseStrength, speedScaling;

    private void Update()
    {
        float strength = Mathf.Abs(playerMovement.Velocity.x) - minSpeedThresold;
        if (strength < 0 || playerMovement.SpecialState == SpecialState.Rocket) strength = 0;
        strength *= speedScaling;
        var emission = particle.emission;
        emission.rateOverTimeMultiplier = baseStrength * strength;

    }
}
