using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using System.Collections;

public class RoomManager : Singleton<RoomManager>
{
    public Room activeRoom;

	private Doorway previousDoorway;
	private Room previousRoom;

    public int BaseWidth = 64, BaseHeight = 36;

    public int TransitionWidth = 4;
    public int TransitionFadeFrames = 20; // per side of room

    public BoxCollider2D GuideRailPrefab;
    public BoxCollider2D FreezeTriggerPrefab;

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
        StartCoroutine(SwitchRoomCoroutine(door1, door2));
    }

    private IEnumerator SwitchRoomCoroutine(Doorway door1, Doorway door2)
    {

        PlayerMovement pm = Player.Instance.Movement;
        Vector2 dir = door1.GetTransitionDirection();
        Vector2 preservedVelocity;

        // on up transitions, give player a boost
        if (dir == Vector2.up)
        {
            pm.GravityEnabled = false;
            const float upBoost = 1.5f;
            if (pm.Velocity.y < pm.MovementParams.JumpSpeed * upBoost)
            {
                pm.Velocity = new(pm.Velocity.x, pm.MovementParams.JumpSpeed * upBoost);
            }
        }
        preservedVelocity = pm.Velocity;
        pm.EndJump(true);


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

		previousRoom = activeRoom;
		previousDoor = door1.enclosedRoom;
        activeRoom = door2.enclosingRoom;
        Vector3 newPosition = door2.enclosingRoom.transform.position + new Vector3(BaseWidth / 2, BaseHeight / 2);
        newPosition.z = Camera.main.transform.position.z;
        Room room1 = door1.enclosingRoom;
        Room room2 = door2.enclosingRoom;
        PInput.Instance.EnableControls = false;
        if (dir.y == 0)
        {
            PInput.Instance.MoveInputOverrride = dir;
        }
        else if (dir.y == 1)
        {
            // going upwards: force move in facing direction to get to a ledge
            PInput.Instance.MoveInputOverrride = pm.FacingDir;
        }

        FadeToBlack.Instance.FadeIn();
        for (int i = 0; i < TransitionFadeFrames; i++)
        {
            yield return new WaitForFixedUpdate();
        }


        // ** MOVE CAMERA HERE ** 
        Camera.main.transform.position = newPosition;
        // Switch confiner to the one in the new room

        FadeToBlack.Instance.FadeOut();
        for (int i = 0; i < TransitionFadeFrames; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        // suppress target trigger to avoid transitioning back
        door2.SuppressNextTransition();
        pm.transform.position = (Vector2)door2.transform.position + relativePos;
        pm.Locked = false;
        pm.SpecialState = SpecialState.Normal;
        const float minTransitionSpeed = 8;

        // give some minimum velocity entering the room
        if (Vector2.Dot(preservedVelocity, dir) < minTransitionSpeed)
        {
            preservedVelocity = preservedVelocity - dir * Vector2.Dot(preservedVelocity, dir) + dir * minTransitionSpeed;
        }
        pm.Velocity = preservedVelocity;
        if (dir == Vector2.up)
        {
            // set one more time in case of jank
            pm.GravityEnabled = false;
        }
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        PInput.Instance.EnableControls = true;
        PInput.Instance.MoveInputOverrride = Vector2.zero;
        pm.GravityEnabled = true;
    }
}
