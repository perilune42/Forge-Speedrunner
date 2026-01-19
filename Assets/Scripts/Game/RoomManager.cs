using UnityEngine;
using System.Collections.Generic;

public class RoomManager : Singleton<RoomManager>
{
    public Room activeRoom;

    public int BaseWidth = 64, BaseHeight = 36;

    void Start()
    {
        Passage[] allPassages = GetComponentsInChildren<Passage>();

        foreach(Passage pass in allPassages)
        {
            pass.door1.passage = pass;
            pass.door2.passage = pass;
        }
    }

    void Update()
    {
        // switch rooms by arrow keys
        List<Doorway> doorways = null;
        if (Input.GetKeyDown(KeyCode.UpArrow)) 
        {
            doorways = activeRoom.doorwaysUp;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) 
        {
            doorways = activeRoom.doorwaysDown;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) 
        {
            doorways = activeRoom.doorwaysLeft;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) 
        {
            doorways = activeRoom.doorwaysRight;
        }
        if (doorways == null)
        {
            return;
        }

        foreach (Doorway door in doorways)
        {
            if (door != null && door.enclosingRoom == activeRoom)
            {
                Passage pass = door.passage;
                Doorway start = door;
                Doorway end = start == pass.door1? pass.door2 : pass.door1;
                SwitchRoom(start, end);
                return;
            }

        }

        // only if no door matches
        Debug.Log("you can't go this way.");
        return;
    }

    public void SwitchRoom(Doorway door1, Doorway door2)
    {
        // no moving player for now. but moving camera

        Debug.Log($"Switch from room {door1.enclosingRoom} to room {door2.enclosingRoom}");

        activeRoom = door2.enclosingRoom;
        Vector3 newPosition = door2.enclosingRoom.transform.position + new Vector3(BaseWidth / 2, BaseHeight / 2);
        newPosition.z = Camera.main.transform.position.z;
        Camera.main.transform.position = newPosition;
    }
}
