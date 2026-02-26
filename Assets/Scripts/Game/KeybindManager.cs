using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class KeybindManager : Singleton<KeybindManager>
{
    public Dictionary<InputBinding, string> bindingStrings = new Dictionary<InputBinding, string>();
    public Dictionary<InputBinding, InputAction> bindingsToActions = new Dictionary<InputBinding, InputAction>();
    public Action OnInputChange;
    public bool UsingGamepad;

    public override void Awake()
    {
        base.Awake();
        bool containsGamepad = false;
        foreach (InputDevice dev in InputSystem.devices)
        {
            if (dev.name.Equals("Gamepad")) containsGamepad = true;
        }
        LoadKeybindStrings(containsGamepad);
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
                InputBinding binding = action.bindings[i];
                if (!IsValidPath(binding.path, gamepad)) continue;
                if (action.actionMap.name != "Player") continue;
                string bindingString = Util.FixControlString(action.GetBindingDisplayString(i), binding);
                bindingStrings[binding] = bindingString;
                bindingsToActions[binding] = action;
            }
        }
        UsingGamepad = gamepad;
        OnInputChange?.Invoke();
    }

    private bool IsValidPath(string path, bool gamepad)
    {
        return (path.StartsWith("<Keyboard>") && !gamepad)
               || (path.StartsWith("<Gamepad>") && gamepad);
    }

    public InputBinding GetBindingFromAction(InputAction action)
    {
        return action.bindings[GetBindingIndexFromAction(action)];
    }

    public int GetBindingIndexFromAction(InputAction action)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];
            if (binding.path.StartsWith("<Gamepad>") && UsingGamepad) return i;
            else if (binding.path.StartsWith("<Keyboard>") && !UsingGamepad) return i;
        }
        return 0;
    }

    public int GetIndexOfBinding(InputBinding binding, InputAction action)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding b = action.bindings[i];
            if (b.path.Equals(binding.path)) return i;
        }
        return 0;
    }
}
