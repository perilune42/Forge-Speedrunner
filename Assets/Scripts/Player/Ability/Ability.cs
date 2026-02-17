using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public struct AbilityLevel
{
    public string Description;
    public int Cost;
}

public abstract class Ability : MonoBehaviour
{
    [Header("Ability Metadata")]

    public string Name;
    public Sprite Icon;

    public AbilityLevel[] AllLevels;
    public int ID;
    
    public int CurrentLevel;    // 0-indexed

    [HideInInspector] public int CurCharges;
    public int MaxCharges;
    public bool UsesCharges = false;
    public bool StartUnlocked;

    [HideInInspector] public string BindingDisplayString;

    [Header("===========")]

    [SerializeField] protected int cooldown;
    protected int curCooldown;

    protected PlayerMovement PlayerMovement => Player.Instance.Movement;
    protected PlayerVFXTrail PlayerVFXTrail => Player.Instance.VFXTrail;
    public AbilityInfo info;

    public Action OnActivate;
    protected Action stopParticleAction;

    protected PInput.InputButton inputButton;

    protected virtual void Awake()
    {
        
    }
    


    public virtual void Start()
    {
        if (AbilityManager.Instance.AbilityInfoParent == null) return;
        info = Instantiate(AbilityManager.Instance.AbilityInfoPrefab, 
            AbilityManager.Instance.AbilityInfoParent.transform).GetComponent<AbilityInfo>();
        info.SetAbility(this);
        if (this is Dash)
        {
            SetInputButton(PInput.Instance.Dash);
        }
        else
        {
            SetInputButton(PInput.Instance.AddAbilityInputButton());
        }
        UpdateBindingText(inputButton.GetAction());
    }

    protected virtual void FixedUpdate()
    {
        if (CanRecharge() && curCooldown > 0) 
        {
            curCooldown--;
            if (curCooldown == 0) onRecharged?.Invoke();
        }
    }

    public Action onRecharged;

    // set to false to halt cooldown ticking
    protected virtual bool CanRecharge() => true;
    
    /// <summary>
    /// Cooldown for this ability, as a float between 0.0 and 1.0
    /// <para> If ability is available, this should return 1.0 </para>
    /// </summary>
    public virtual float GetCooldown()
    {
        return ((float)(cooldown - curCooldown)) / cooldown;
    }

    /// <summary>
    /// Whether this ability is available, regardless of cooldown
    /// </summary>
    public virtual bool CanUseAbility()
    {
        if (PlayerMovement.SpecialState == SpecialState.Rocket && this is not Ricochet && this is not Recall) return false;
        if (PlayerMovement.SpecialState == SpecialState.Teleport) return false;
        if (UsesCharges)
        {
            return CurCharges > 0;
        }
        else
        {
            return curCooldown == 0;
        }
            
    }
    
    public virtual bool UseAbility()
    {
        if (UsesCharges)
        {
            CurCharges--;
        }
        else
        {
            curCooldown = cooldown;
        }
        OnActivate?.Invoke();
        return false;
    }

    public void Recharge()
    {
        curCooldown = 0;
        onRecharged?.Invoke();
    }

    public void SetInputButton(PInput.InputButton button)
    {
        inputButton = button;
    }

    public void UpdateBindingText(InputAction action)
    {
        if (inputButton.GetAction().Equals(action))
        {
            Debug.Log("Updated binding display string for ability");
            BindingDisplayString = KeybindManager.Instance.bindingStrings[action];
        }
    }

    public void UpdateBindingText()
    {
        UpdateBindingText(inputButton.GetAction());
    }

    [ContextMenu("Reset")]
    public virtual void OnReset() { }
}
