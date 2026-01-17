using UnityEngine;
using System.Collections.Generic;

public class RoomManager : Singleton<RoomManager>
{
    [SerializeField] Room activeRoom;

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

        Doorway start = null; Doorway end = null;
        foreach (Doorway door in doorways)
        {
            if(door == null)
            {
                continue;
            }
            if (door.enclosingRoom == activeRoom)
            {
                Passage pass = door.passage;
                start = door;
                end = start == pass.door1? pass.door2 : pass.door1;
                break;
            }

        }
        if (start == null) 
        {
            Debug.Log("you can't go this way.");
            return;
        }

        SwitchRoom(start, end);
    }

    public void SwitchRoom(Doorway door1, Doorway door2)
    {
        // no moving player for now. but moving camera

        Debug.Log($"Switch from room {door1.enclosingRoom} to room {door2.enclosingRoom}");

        activeRoom = door2.enclosingRoom;
        Vector3 newPosition = door2.enclosingRoom.transform.position;
        newPosition.z = Camera.main.transform.position.z;
        Camera.main.transform.position = newPosition;
    }
}
