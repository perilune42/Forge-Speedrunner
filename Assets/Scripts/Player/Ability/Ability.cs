using System;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Ability : MonoBehaviour
{
    
    /// <summary>
    /// Used for UI in shops and stuff.
    /// Includes the name, description, and icon of the ability.
    /// </summary>
    public AbilityData Data;

    [HideInInspector] public int ID;
    
    [HideInInspector] public int Level;
    protected PlayerMovement PlayerMovement => Player.Instance.Movement;
    private AbilityInfo info;

    public Action OnActivate;

    protected virtual void Awake()
    {
        
    }
    

    public virtual void Start()
    {
        Level = AbilitySceneTransfer.AbilityDataArray[ID].Level;
        if (AbilityManager.Instance.AbilityInfoParent == null) return;
        info = Instantiate(AbilityManager.Instance.AbilityInfoPrefab, 
            AbilityManager.Instance.AbilityInfoParent.transform).GetComponent<AbilityInfo>();
        info.Ability = this;
    }

    
    /// <summary>
    /// Cooldown for this ability, as a float between 0.0 and 1.0
    /// <para> If ability is available, this should return 1.0 </para>
    /// </summary>
    public abstract float GetCooldown();

    /// <summary>
    /// Whether this ability is available, regardless of cooldown
    /// </summary>
    public abstract bool CanUseAbility();
    
    public virtual bool UseAbility()
    {
        OnActivate?.Invoke();
        return false;
    }
}
