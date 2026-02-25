using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class AbilitySlot : MonoBehaviour, IPointerDownHandler
{
    // Whoever wrote this code, talked shit on my code
    // That was really hurtful, whoever that was
    [SerializeField] string inputName; // jank because InputActionReference is bugged on our version of the InputSystem
    // If we ever update the packages, we can use InputActionReferences instead
    private InputAction inputAction;
    [SerializeField] public TMP_Text bindingText;

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
        if (ShopAbility.SelectedAbility != null)
        {
            // TODO - actually swap ability slots
            // if self, return
            // if other, swap
            // if empty, move
            Debug.Log("Swap Abilities!");
            ShopAbility.SelectedAbility = null;
        }
    }
}
