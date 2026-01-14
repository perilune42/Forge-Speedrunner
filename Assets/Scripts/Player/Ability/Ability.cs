using System;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Ability : MonoBehaviour
{
    [SerializeField] private InputActionReference actionReference;
    /// <summary>
    /// Used for UI in shops and stuff.
    /// Includes the name, description, and icon of the ability.
    /// More fields may be added once ability leveling is implemented.
    /// </summary>
    public AbilityData data;
    [HideInInspector] public int Level;
    public PlayerMovement PlayerMovement;
    
    /// <summary>
    /// Calls starting code for this Ability.
    /// Should be used instead of Awake() or Start(),
    /// since PlayerMovement is still null when those methods
    /// are called.
    /// </summary>
    public virtual void Start()
    {
        PlayerMovement = GameObject.FindFirstObjectByType<PlayerMovement>();
        actionReference.action.performed += context => UseAbility();
        actionReference.action.Enable();
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

    public void TryUseAbility()
    {
        if (GetCooldown() >= 1f)
        {
            UseAbility();
        }
    }
    
    public abstract bool UseAbility();
}
