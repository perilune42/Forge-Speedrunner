using UnityEngine;

public class RoomManager : Singleton<RoomManager>
{


    public void SwitchRoom(Doorway door1, Doorway door2)
    {
        // no moving player for now

        Debug.Log($"Switch from room {door1.enclosingRoom} to room {door2.enclosingRoom}");
    }
}
