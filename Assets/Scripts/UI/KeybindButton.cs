using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeybindButton : MonoBehaviour
{
    [HideInInspector] public InputAction inputAction;
    [HideInInspector] public int index;
    public TMP_Text nameText;
    public TMP_Text bindText;
    void Start()
    {
        nameText.text = inputAction.name + " " + inputAction.bindings[index].name;
        bindText.text = Util.FixControlString(inputAction.GetBindingDisplayString(index), inputAction, index);
        
    }

    public void RemapKeybind()
    {
        inputAction.Disable();
        var rebindingOperation = inputAction.PerformInteractiveRebinding(index);
        bindText.text = "Press any key";
        rebindingOperation.OnComplete(
            operation =>
            {
                Debug.Log($"Rebound {inputAction} to {operation.selectedControl}");
                string bindingString = Util.FixControlString(inputAction.GetBindingDisplayString(index), inputAction, index);
                bindText.text = bindingString;
                KeybindManager.Instance.bindingStrings[inputAction] = bindingString;
                foreach (Ability ability in AbilityManager.Instance.GetAllAbilities())
                {
                    ability.UpdateBindingText(inputAction);
                }
                inputAction.Enable();
                operation.Dispose();
            });
        rebindingOperation.Start();
            
    }
}