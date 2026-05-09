using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ShopAbilitySlot : MonoBehaviour, IPointerDownHandler
{
    // Whoever wrote this code, talked shit on my code
    // That was really hurtful, whoever that was
    // ^ I'M SO SORRY IF IT WAS ME IT WASN'T PERSONAL
    [SerializeField] string inputName; // jank because InputActionReference is bugged on our version of the InputSystem
    // If we ever update the packages, we can use InputActionReferences instead
    private InputAction inputAction;
    [SerializeField] public TMP_Text bindingText;
    [SerializeField] public AbilitySlotID SlotID;

    private void Awake()
    {
        KeybindManager.Instance.OnInputChange += UpdateKeybinds;
        foreach (var action in InputSystem.ListEnabledActions())
        {
            if (action.name == inputName)
            {
                inputAction = action;
            }
        }
    }

    void OnEnable()
    {
        UpdateKeybinds();
    }

    private void UpdateKeybinds()
    {

        bindingText.text = KeybindManager.Instance.bindingStrings[
            KeybindManager.Instance.GetBindingFromAction(inputAction)];
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.ClickAbilitySlot(this);
        }
        else
        {
            StartingAbilityManager.Instance.ClickAbilitySlot(this);
        }
    }
}
