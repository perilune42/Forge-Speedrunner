using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using Unity.Cinemachine;
using System.Collections;

public class RoomManager : Singleton<RoomManager>
{
    public Room activeRoom;

    private Doorway previousDoorway;
    private Vector2 relativePos = new Vector2(0.0F, 0.0F);

    private Vector2 originalPosition;
    private Room originalRoom;

    public int BaseWidth = 64, BaseHeight = 36;

    public int TransitionWidth = 4;
    public int TransitionFadeFrames = 20; // per side of room

    // moved to CameraController.cs
    //public BoxCollider2D CameraBounds;
    //public CinemachineConfiner2D CameraConfiner;

    // we should move these prefabs out of here...
    public BoxCollider2D GuideRailPrefab;
    public BoxCollider2D FreezeTriggerPrefab;
    public BoxCollider2D BlockerHorzPrefab, BlockerVertPrefab;
    public InteractionTrigger InteractionTriggerPrefab;

    public List<Room> AllRooms = new();
    [HideInInspector] public ActivatableEntity[] ActivatableEntities;
    [HideInInspector] public Passage[] AllPassages;

    public Vector2 RespawnPosition;

    private Room findActiveRoom(List<Room> allRooms)
    {
        Vector3 playerPos = Player.Instance.Movement.transform.position;
        // 1. calculations to find room enclosing player
        Room targetRoom = null;
        foreach(Room room in allRooms)
        {
            Vector3 botleft = room.transform.position;
            Vector2Int sizeWorld = room.size * new Vector2Int(66, 38);
            Vector3 topright = botleft;
            topright.x += sizeWorld.x;
            topright.y += sizeWorld.y;
            bool betweenX = botleft.x <= playerPos.x && playerPos.x <= topright.x;
            bool betweenY = botleft.y <= playerPos.y && playerPos.y <= topright.y;
            if(betweenX && betweenY)
            {
                targetRoom = room;
                break;
            }
        }
        // 2. return targetRoom
        return targetRoom;
    }

    void Start()
    {
        // Change index of GetChild based on the index of the Passages object's 
        // index in the children hierarchy
        AllPassages = transform.GetChild(0).GetComponentsInChildren<Passage>();

        // AllRooms = GetComponentsInChildren<Room>().ToList();

        ActivatableEntities = GetComponentsInChildren<ActivatableEntity>();

        activeRoom = findActiveRoom(AllRooms);

        originalPosition = Player.Instance.Movement.transform.position;
        RespawnPosition = originalPosition;
        originalRoom = activeRoom;

        foreach(Passage pass in AllPassages)
        {
            pass.door1.passage = pass;
            pass.door2.passage = pass;
        }
        CameraController.Instance.SnapToRoom(activeRoom);
    }

    public void ResetAllEntities()
    {
        // reset entities
        foreach(ActivatableEntity e in ActivatableEntities)
        {
            e.ResetEntity();
        }

        // reset player
        PlayerMovement pm = Player.Instance.Movement;
        pm.transform.position = originalPosition;
        pm.Velocity = new(0.0F, 0.0F);

        // reset camera and room data
        previousDoorway = null;
        relativePos = new(0.0F, 0.0F);
        CameraController.Instance.SnapToRoom(activeRoom);
    }

    void Update()
    {
        // debug inputs moved to Game
        return;
    }

    public void SwitchRoom(Doorway door1, Doorway door2)
    {
        StartCoroutine(SwitchRoomCoroutine(door1, door2));
    }
    
    // respawn location set by safe zones
    // todo: add fallback in case player hasn't touched one in this room
    public void Respawn()
    {
        StartCoroutine(roomTransition(activeRoom));
        StartCoroutine(warpTo(RespawnPosition, Vector2.zero, Vector2.zero, 30));
    }

    public void ReEnterRoom()
    {
        PlayerMovement pm = Player.Instance.Movement;

        if (previousDoorway == null)
        {
            return;
        }


        // come from the reverse direction this time
        Vector2 dir = previousDoorway.GetTransitionDirection() * -1;

        // no velocity for respawning
        Vector2 preservedVelocity = 1.0F * dir;

        const float upBoost = 1.5f;
        if (dir == Vector2.up
                && pm.Velocity.y < pm.MovementParams.JumpSpeed * upBoost)
        {
            preservedVelocity.y += pm.MovementParams.JumpSpeed * upBoost;
        }

        Debug.Log($"Respawning in {previousDoorway}. doorway is in {previousDoorway.enclosingRoom}, hopefully expected!");
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

        // calculate relativePos, which is unavoidable
        relativePos = pm.transform.position - door1.transform.position;

        previousDoorway = door2;
        activeRoom = door2.enclosingRoom;


        return warpTo(door2, preservedVelocity, dir);
    }
    private IEnumerator warpTo(Doorway door2, Vector2 preservedVelocity, Vector2 dir)
    {

        // assuming doorways are placed correctly in world space
        // i.e. centered properly along the world grid
        // NOTE: the code below works every time for switchroom, but for respawning it is problematic.
        // if (door2.IsHorizontal())
        // {
        //     relativePos = new Vector2(0, pm.transform.position.y - door2.transform.position.y);
        // }
        // else
        // {
        //     relativePos = new Vector2(pm.transform.position.x - door2.transform.position.x, 0);
        // }

        // calculate new player position
        Vector2 newPlayerPos = (Vector2)door2.transform.position;
        if (door2.IsHorizontal())
            newPlayerPos.y += relativePos.y;
        else
            newPlayerPos.x += relativePos.x;

        /**
         * start doing things to the player here
         */
        door2.SuppressNextTransition();
        // do these sequentially. looks clearer
        yield return roomTransition(door2.enclosingRoom);
        yield return warpTo(newPlayerPos, preservedVelocity, dir);
        door2.EnableTransition();
    }
    public IEnumerator warpTo(Vector2 position, Vector2 preservedVelocity, Vector2 dir, int lockDuration = 10)
    {
        PlayerMovement pm = Player.Instance.Movement;


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

        // suppress target trigger to avoid transitioning back
        pm.transform.position = position;
        pm.Locked = false;
        pm.SpecialState = SpecialState.Normal;
        const float minTransitionSpeed = 0;

        // give some minimum velocity entering the room
        if (dir != Vector2.zero && Vector2.Dot(preservedVelocity, dir) < minTransitionSpeed)
        {
            preservedVelocity = preservedVelocity - dir * Vector2.Dot(preservedVelocity, dir) + dir * minTransitionSpeed;
        }
        if (dir != Vector2.zero)
        {
            pm.Velocity = preservedVelocity;
        }
        else
        {
            pm.Velocity = Vector2.zero;
        }
        if (dir == Vector2.up)
        {
            // set one more time in case of jank
            pm.GravityEnabled = false;
        }
        for (int i = 0; i < lockDuration; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        PInput.Instance.EnableControls = true;
        PInput.Instance.MoveInputOverrride = Vector2.zero;
        pm.GravityEnabled = true;
    }

    private IEnumerator roomTransition(Room room)
    {
        Debug.Log("start room transition");
        FadeToBlack.Instance.FadeIn();
        for (int i = 0; i < TransitionFadeFrames; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        // logic moved to CameraController
        CameraController.Instance.SnapToRoom(room);

        // 3 cope frames
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        FadeToBlack.Instance.FadeOut();
        for (int i = 0; i < TransitionFadeFrames; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        Debug.Log("end room transition");
    }
}
