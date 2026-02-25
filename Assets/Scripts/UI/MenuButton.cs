using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        RuntimeManager.PlayOneShot("event:/UI Click");
    }
}
