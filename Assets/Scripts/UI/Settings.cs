using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Settings : MonoBehaviour
{
    [SerializeField] private GameObject keybindButton, keybindButtonParent;
    private List<GameObject> buttons = new();

    void Start()
    {
        InitKeybindButtons();
        InputSystem.onDeviceChange += (device, change) =>
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    if (device is Gamepad) InitKeybindButtons();
                    break;

                case InputDeviceChange.Removed:
                    if (device is Gamepad) InitKeybindButtons();
                    break;
            }
        };
    }

    private void InitKeybindButtons()
    {
        foreach (GameObject button in buttons) Destroy(button);
        buttons.Clear();
        foreach (InputAction action in InputSystem.ListEnabledActions())
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (!IsValidPath(action.bindings[i].path)) continue;
                KeybindButton kb = Instantiate(keybindButton, keybindButtonParent.transform).GetComponent<KeybindButton>();
                kb.inputAction = action;
                kb.index = i;
                buttons.Add(kb.gameObject);
            }
        }
    }

    private bool IsValidPath(string path)
    {
        return path.StartsWith("<Keyboard>");
    }
}
