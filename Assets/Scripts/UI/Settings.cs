using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Settings : MonoBehaviour
{
    [SerializeField] private GameObject keybindButton, keybindButtonParent;
    private List<GameObject> buttons = new();

    void Start()
    {
        InitKeybindButtons(false);
        InputSystem.onDeviceChange += (device, change) =>
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    if (device is Gamepad) InitKeybindButtons(true);
                    break;

                case InputDeviceChange.Removed:
                    if (device is Gamepad) InitKeybindButtons(false);
                    break;
            }
        };
    }

    private void InitKeybindButtons(bool gamepad)
    {
        foreach (GameObject button in buttons) Destroy(button);
        buttons.Clear();
        foreach (KeyValuePair<InputAction, string> kv in KeybindManager.Instance.bindingStrings)
        {
            KeybindButton kb = Instantiate(keybindButton, keybindButtonParent.transform).GetComponent<KeybindButton>();
            kb.inputAction = kv.Key;
            buttons.Add(kb.gameObject);
        }
    }

    private bool IsValidPath(string path, bool gamepad)
    {
        return (path.StartsWith("<Keyboard>") && !gamepad) 
               || (path.StartsWith("<Gamepad>") && gamepad);
    }
}
