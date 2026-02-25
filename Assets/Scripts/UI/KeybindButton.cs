using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeybindButton : MonoBehaviour
{
    private InputAction inputAction;
    [HideInInspector] public InputBinding InputBinding;
    [HideInInspector] public int index;
    public TMP_Text nameText;
    public TMP_Text bindText;
    void Start()
    {
        inputAction = KeybindManager.Instance.bindingsToActions[InputBinding]; // could also be done using the input path strings, but this is easier
        index = KeybindManager.Instance.GetIndexOfBinding(InputBinding, inputAction);
        nameText.text = inputAction.name + " " + InputBinding.name;
        bindText.text = Util.FixControlString(InputBinding.ToDisplayString(), InputBinding);
        
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
                InputBinding = inputAction.bindings[index]; // needed or else it has the wrong binding for some reason
                string bindingString = Util.FixControlString(InputBinding.ToDisplayString(), InputBinding);
                bindText.text = bindingString;
                KeybindManager.Instance.bindingStrings[InputBinding] = bindingString;
                foreach (Ability ability in AbilityManager.Instance.GetAllAbilities())
                {
                    ability.UpdateBindingText();
                }
                AbilityManager.Instance.GetAbility<Chronoshift>().UpdateBindingText();
                
                inputAction.Enable();
                operation.Dispose();
            });
        rebindingOperation.Start();
            
    }
}