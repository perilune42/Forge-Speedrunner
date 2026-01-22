using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;

public class RoomManager : Singleton<RoomManager>
{
    public Room activeRoom;

    public int BaseWidth = 64, BaseHeight = 36;

    public List<Room> AllRooms = new();

    void Start()
    {
        Passage[] allPassages = GetComponentsInChildren<Passage>();
        AllRooms = GetComponentsInChildren<Room>().ToList();

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
        PlayerMovement pm = Player.Instance.Movement;
        Vector2 dir = door1.GetTransitionDirection();
        Vector2 preservedVelocity = pm.Velocity;
        pm.Velocity = Vector2.zero;
        Vector2 relativePos;
        // assuming doorways are placed correctly in world space
        // i.e. centered properly along the world grid
        if (door1.IsHorizontal())
        {
            relativePos = new Vector2(0, pm.transform.position.y - door1.transform.position.y);
        }
        else
        {
            relativePos = new Vector2(pm.transform.position.x - door1.transform.position.x, 0);
        }
        pm.SpecialState = SpecialState.Normal;  // todo: preserve some states such as ground slam



        // no moving player for now. but moving camera

        Debug.Log($"Switch from room {door1.enclosingRoom} to room {door2.enclosingRoom}");

        activeRoom = door2.enclosingRoom;
        Vector3 newPosition = door2.enclosingRoom.transform.position + new Vector3(BaseWidth / 2, BaseHeight / 2);
        newPosition.z = Camera.main.transform.position.z;
        Room room1 = door1.enclosingRoom;
        Room room2 = door2.enclosingRoom;

        Camera.main.transform.position = newPosition;
        // Switch confiner to the one in the new room


        // suppress target trigger to avoid transitioning back
        door2.SuppressNextTransition();
        pm.transform.position = (Vector2)door2.transform.position + relativePos;
        const float minTransitionSpeed = 4;

        // give some minimum velocity entering the room
        if (Vector2.Dot(preservedVelocity, dir) < minTransitionSpeed)
        {
            preservedVelocity = preservedVelocity - dir * Vector2.Dot(preservedVelocity, dir) + dir * minTransitionSpeed;
        }
        pm.Velocity = preservedVelocity;
    }
}
