using UnityEngine;

public class Doorway : MonoBehaviour 
{
    [HideInInspector] public Room enclosingRoom;
    [HideInInspector] public Passage passage;

    private bool suppressTransition = false;

    void Start()
    {
        enclosingRoom = GetComponentInParent<Room>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() == null || suppressTransition) return;
        var otherDoor = this == passage.door1? passage.door2 : passage.door1; 
        RoomManager.Instance.SwitchRoom(this, otherDoor);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>() != null)
        {
            suppressTransition = false;
        }
    }

    // true if this is a L/R transition
    public bool IsHorizontal()
    {
        return enclosingRoom.doorwaysLeft.Contains(this) || enclosingRoom.doorwaysRight.Contains(this);
    }

    public Vector2 GetTransitionDirection()
    {
        if (enclosingRoom.doorwaysLeft.Contains(this)) return Vector2.left;
        else if (enclosingRoom.doorwaysRight.Contains(this)) return Vector2.right;
        else if (enclosingRoom.doorwaysUp.Contains(this)) return Vector2.up;
        else if (enclosingRoom.doorwaysDown.Contains(this)) return Vector2.down;
        return Vector2.zero;
    }

    // suppress one activation until player exits
    public void SuppressNextTransition()
    {
        suppressTransition = true;
    }
}
