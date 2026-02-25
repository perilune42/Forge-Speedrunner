using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class AbilitySlot : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] string inputName;  // scuffed

    [SerializeField] public TMP_Text bindingText;

    private void Awake()
    {
        KeybindManager.Instance.OnInputChange += UpdateKeybinds;
        UpdateKeybinds();
    }

    private void UpdateKeybinds()
    {
        // wtf is this
        foreach (var action in InputSystem.ListEnabledActions())
        {
            if (action.name == inputName)
            {
                bindingText.text = KeybindManager.Instance.bindingStrings[action];
            }
        }
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
