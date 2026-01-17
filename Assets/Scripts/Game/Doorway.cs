using UnityEngine;

public class Doorway : MonoBehaviour 
{
    public Room enclosingRoom;
    public Passage passage;

    void OnTriggerEnter2D(Collider2D other)
    {
        var otherDoor = this == passage.door1? passage.door2 : passage.door1; 
        RoomManager.Instance.SwitchRoom(this, otherDoor);
    }
}
