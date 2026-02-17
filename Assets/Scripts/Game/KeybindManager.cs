using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class KeybindManager : Singleton<KeybindManager>
{
    public Dictionary<InputAction, string> bindingStrings = new Dictionary<InputAction, string>();
    
    public override void Awake()
    {
        base.Awake();
        LoadKeybindStrings(false);
        InputSystem.onDeviceChange += (device, change) =>
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    if (device is Gamepad) LoadKeybindStrings(true);
                    break;

                case InputDeviceChange.Removed:
                    if (device is Gamepad) LoadKeybindStrings(false);
                    break;
            }
        };
    }
    
    private void LoadKeybindStrings(bool gamepad)
    {
        foreach (InputAction action in InputSystem.ListEnabledActions())
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (!IsValidPath(action.bindings[i].path, gamepad)) continue;
                if (action.actionMap.name != "Player") continue;
                string bindingString = Util.FixControlString(action.GetBindingDisplayString(i), action, i);
                bindingStrings[action] = bindingString;
            }
        }
    }

    private bool IsValidPath(string path, bool gamepad)
    {
        return (path.StartsWith("<Keyboard>") && !gamepad) 
               || (path.StartsWith("<Gamepad>") && gamepad);
    }
}
