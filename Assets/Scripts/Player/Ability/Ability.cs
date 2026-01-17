using System;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Ability : MonoBehaviour
{
    
    /// <summary>
    /// Used for UI in shops and stuff.
    /// Includes the name, description, and icon of the ability.
    /// More fields may be added once ability leveling is implemented.
    /// </summary>
    public AbilityData Data;
    [HideInInspector] public int Level;
    protected PlayerMovement PlayerMovement => Player.Instance.Movement;
    
    public virtual void Start()
    {
    }

    protected virtual void Update()
    {
        // get cooldown UI stuff
    }
    
    /// <summary>
    /// Cooldown for this ability, as a float between 0.0 and 1.0
    /// <para> If ability is unavailable, this should return 0.0 </para>
    /// </summary>
    public abstract float GetCooldown();

    public virtual bool CanUseAbility()
    {
        return GetCooldown() >= 1f;
    }
    
    public abstract bool UseAbility();
}
