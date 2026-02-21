using UnityEngine;
using UnityEngine.EventSystems;

public class AbilitySlot : MonoBehaviour, IPointerDownHandler
{
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
