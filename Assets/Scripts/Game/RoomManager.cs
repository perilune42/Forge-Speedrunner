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

    // this should not be required.
    private Vector2 originalPosition;

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
        originalPosition = Player.Instance.Movement.transform.position;

    }

    void Update()
    {
        // switch rooms by arrow keys
        List<Doorway> doorways = null;
        if(Input.GetKeyDown(KeyCode.R))
            Respawn();
        else if (Input.GetKeyDown(KeyCode.UpArrow)) 
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
    public void Respawn()
    {
        PlayerMovement pm = Player.Instance.Movement;

        // in this case, skip most of this. just teleport back to placement position
        if (previousDoorway == null)
        {
            return;
        }

        // come from the reverse direction this time.
        Vector2 dir = previousDoorway.GetTransitionDirection() * -1;

        // no velocity for respawning
        Vector2 preservedVelocity = 1.0F * dir;

        Debug.Log($"Respawning in {previousDoorway}, room {previousRoom}. doorway is in {previousDoorway.enclosingRoom}, hopefully expected!");
        activeRoom = previousRoom;
        StartCoroutine(warpTo(previousDoorway, preservedVelocity, dir));
    }

    private IEnumerator SwitchRoomCoroutine(Doorway door1, Doorway door2)
    {
        PlayerMovement pm = Player.Instance.Movement;

        // calculate player velocity
        Vector2 dir = door1.GetTransitionDirection();
        Vector2 preservedVelocity;

        // on up transitions, give player a boost
        const float upBoost = 1.5f;
        if (dir == Vector2.up
                && pm.Velocity.y < pm.MovementParams.JumpSpeed * upBoost)
        {
            preservedVelocity = new(pm.Velocity.x, pm.MovementParams.JumpSpeed * upBoost);
        }
        else preservedVelocity = pm.Velocity;
        Debug.Log($"Switch from room {door1.enclosingRoom} to room {door2.enclosingRoom}");
        previousRoom = activeRoom;
        previousDoorway = door1;
        activeRoom = door2.enclosingRoom;
        return warpTo(door2, preservedVelocity, dir);
    }
    private IEnumerator warpTo(Doorway door2, Vector2 preservedVelocity, Vector2 dir)
    {
        PlayerMovement pm = Player.Instance.Movement;
        Vector2 relativePos;

        // assuming doorways are placed correctly in world space
        // i.e. centered properly along the world grid
        // NOTE: if unaligned, this would probably throw player into a very weird position.
        if (door2.IsHorizontal())
        {
            relativePos = new Vector2(0, pm.transform.position.y - door2.transform.position.y);
        }
        else
        {
            relativePos = new Vector2(pm.transform.position.x - door2.transform.position.x, 0);
        }

        // calculate new player position
        Vector2 newPlayerPos = (Vector2)door2.transform.position + relativePos;

        // calculate camera position
        Vector3 newPosition = door2.enclosingRoom.transform.position + new Vector3(BaseWidth / 2, BaseHeight / 2);
        newPosition.z = Camera.main.transform.position.z;

        /**
         * start doing things to the player here
         */

        PInput.Instance.EnableControls = false;
        pm.GravityEnabled = false;
        pm.EndJump(true);
        pm.SpecialState = SpecialState.Normal;  // todo: preserve some states such as ground slam

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
        pm.transform.position = newPlayerPos;
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
