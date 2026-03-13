using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class RoomManager : Singleton<RoomManager>
{
    public Room activeRoom;

    private Doorway previousDoorway;
    private Vector2 relativePos = new Vector2(0.0F, 0.0F);

    private Vector2 originalPosition;
    private Room originalRoom;

    public int BaseWidth = 64, BaseHeight = 36;

    public int TransitionWidth = 4;

    // we should move these prefabs out of here...
    public BoxCollider2D GuideRailPrefab;
    public BoxCollider2D FreezeTriggerPrefab;
    public BoxCollider2D BlockerHorzPrefab, BlockerVertPrefab;
    public InteractionTrigger InteractionTriggerPrefab;

    public GameObject EntranceIndicatorPrefab;

    public List<Room> AllRooms = new();
    [HideInInspector] public ActivatableEntity[] ActivatableEntities;
    [HideInInspector] public Passage[] AllPassages;

    private Vector2 respawnPosition;
    private bool respawnIsSet = false;
    public Room StartingRoom;
    public Transform StartingSpawn;

    public bool TransitionOngoing = false;

    private Vector2 preservedVelocity;
    
    

    public Vector2 RespawnPosition { get => respawnPosition; set { 
            respawnPosition = value;
            respawnIsSet = true;
    } }

    public override void Awake()
    {
        base.Awake();
        AllPassages = transform.GetChild(0).GetComponentsInChildren<Passage>();
    }

    void Start()
    {
        // Change index of GetChild based on the index of the Passages object's 
        // index in the children hierarchy
        

        // AllRooms = GetComponentsInChildren<Room>().ToList();

        ActivatableEntities = GetComponentsInChildren<ActivatableEntity>();
        originalPosition = Player.Instance.Movement.transform.position;
        originalRoom = findActiveRoom(AllRooms);

        // Set room to visited
        originalRoom.visited = true;

        if (Game.Instance.AllRoomsDiscovered)
        {
            foreach (var room in AllRooms)
            {
                room.visited = true;
            }
        }


        foreach (Passage pass in AllPassages)
        {
            if (pass.door1 == null)
            {
                Debug.LogError($"Passage: {pass.name} is invalid!");
            }


            pass.door1.passage = pass;
            pass.door2.passage = pass;
            if (Game.Instance.AllRoomsDiscovered) pass.visited = true;
        }
        
    }

    public void SpawnAtStart(bool allowOverride = false)
    {
        if (allowOverride && Game.Instance.OverrideStartingRoom)
        {
            activeRoom = originalRoom;
            RespawnPosition = originalPosition;
            CameraController.Instance.SnapToRoom(activeRoom);
        }
        else
        {
            activeRoom = StartingRoom;
            originalPosition = StartingSpawn.position;
            RespawnPosition = StartingSpawn.position;
            Player.Instance.Movement.transform.position = StartingSpawn.position;
            originalRoom = StartingRoom;
            CameraController.Instance.SnapToRoom(activeRoom);
            respawnIsSet = false;
        }
    }

    public void SpawnAtDoorway(Doorway door)
    {
        activeRoom = door.enclosingRoom;
        originalPosition = door.transform.position;
        previousDoorway = door;
        originalRoom = activeRoom;
        CameraController.Instance.SnapToRoom(activeRoom);
        respawnIsSet = false;
        Vector2 dir = -previousDoorway.GetTransitionDirection();
        Debug.Log($"Spawning at doorway in room: {activeRoom}");
        preservedVelocity = DeterminePreservedVelocity(dir, true);
        StartCoroutine(GoThroughDoorway(previousDoorway, dir));
    }


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
        preservedVelocity = Vector2.zero;

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

    private Vector2 DeterminePreservedVelocity(Vector2 dir, bool setAsMinimum = false)
    {
        const float minTransitionSpeed = 8;
        var pm = Player.Instance.Movement;
        Vector2 preservedVelocity;
        if (setAsMinimum)
        {
            preservedVelocity = dir * minTransitionSpeed;
        }
        else
        {
            preservedVelocity = pm.Velocity;
        }
        
        // on up transitions, give player a boost
        const float upBoost = 2.5f;
        if (dir == Vector2.up
                && pm.Velocity.y < pm.MovementParams.JumpSpeed * upBoost)
        {
            preservedVelocity = new(pm.Velocity.x, pm.MovementParams.JumpSpeed * upBoost);
        }
        // give some minimum velocity if the player is too slow
        if (dir != Vector2.zero && Vector2.Dot(preservedVelocity, dir) < minTransitionSpeed)
        {
            preservedVelocity = preservedVelocity - dir * Vector2.Dot(preservedVelocity, dir) + dir * minTransitionSpeed;
        }
        return preservedVelocity;
    }
    
    // respawn location set by safe zones
    public void Respawn()
    {
        if (respawnIsSet)   // normal respawn inside a room
        {
            preservedVelocity = Vector2.zero;
            StartCoroutine(RoomTransition(activeRoom, RespawnPosition, Vector2.zero));
        }
        else if (previousDoorway != null)   // respawn not set yet, re-enter room
        {
            Vector2 dir = -previousDoorway.GetTransitionDirection();
            preservedVelocity = DeterminePreservedVelocity(dir, true);
            StartCoroutine(GoThroughDoorway(previousDoorway, dir));
        }
        else // died in starting room before respawn is set
        {
            preservedVelocity = Vector2.zero;
            StartCoroutine(RoomTransition(activeRoom, originalPosition, Vector2.zero));
        }
            
    }

    public void ReEnterRoom()
    {
        PlayerMovement pm = Player.Instance.Movement;

        if (previousDoorway == null)
        {
            StartCoroutine(RoomTransition(activeRoom, originalPosition, Vector2.zero));
            return;
        }


        // come from the reverse direction this time
        Vector2 dir = previousDoorway.GetTransitionDirection() * -1;
        preservedVelocity = DeterminePreservedVelocity(dir, true);
        StartCoroutine(GoThroughDoorway(previousDoorway, dir));
    }

    // usually only used by actual room transitions between 2 doorways
    public void SwitchRoom(Doorway door1, Doorway door2)
    {
        PlayerMovement pm = Player.Instance.Movement;

        // calculate player velocity
        Vector2 dir = door1.GetTransitionDirection();


        // calculate relativePos, which is unavoidable
        relativePos = pm.transform.position - door1.transform.position;

        previousDoorway = door2;
        activeRoom = door2.enclosingRoom;

        // set room to visited
        door1.enclosingRoom.visited = true;
        activeRoom.visited = true;
        door1.passage.visited = true;
        door2.passage.visited = true;
        preservedVelocity = DeterminePreservedVelocity(dir);
        StartCoroutine(GoThroughDoorway(door2, dir));
    }

    // used any time the player pops out of a door
    // preserved velocity should always have been set before entering this
    private IEnumerator GoThroughDoorway(Doorway door2, Vector2 dir)
    {
        //if (!TransitionOngoing)
        {
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
            respawnIsSet = false;
            yield return RoomTransition(door2.enclosingRoom, newPlayerPos, dir);
            door2.EnableTransition();
        }

    }

    // beginning the actual state changes of entering a room
    public IEnumerator RoomTransition(Room room, Vector2 position, Vector2 dir)
    {
        var pm = Player.Instance.Movement;

        TransitionOngoing = true;
        PInput.Instance.EnableControls = false;
        if (dir == Vector2.up)
        {
            Player.Instance.Movement.GravityEnabled = false;
        }
        // give the player a visual boost towards the door (does not affect preserved v)
        pm.Velocity += dir * 5f;

        yield return FadeToBlack.Instance.FadeOut();
        AbilityManager.Instance.ResetAbilites();
        Player.Instance.IsDead = false;

        CameraController.Instance.SnapToRoom(room);
        WarpToPosition(position, dir);

        pm.Locked = true;
        pm.GravityEnabled = true;

        StartCoroutine(FadeToBlack.Instance.FadeIn());  // decouple control locking logic from visual animation
        // apply forced movement on room entry here
        if (dir.y == 0)
        {
            PInput.Instance.MoveInputOverrride = dir;
        }
        else if (dir.y == 1)
        {
            // going upwards: force move in facing direction to get to a ledge
            PInput.Instance.MoveInputOverrride = Player.Instance.Movement.FacingDir;
        }
        const int playerEnterDelay = 40;    // wait this long before player pops out
        const int controlReturnDelay = 20;  // wait this long before player regains control

        if (true || dir != Vector2.zero)
        {
            for (int i = 0; i < playerEnterDelay; i++)
                yield return new WaitForFixedUpdate();
        }
        pm.Locked = false;
        if (dir != Vector2.zero)
        {
            pm.Velocity = preservedVelocity;
        }
        else
        {
            pm.Velocity = Vector2.zero;
        }
        for (int i = 0; i < controlReturnDelay; i++)
            yield return new WaitForFixedUpdate();

        // finally return controls to the player
        PInput.Instance.MoveInputOverrride = Vector2.zero;
        PInput.Instance.EnableControls = true;
        TransitionOngoing = false;
    }

    // while screen is black, move the player here
    public void WarpToPosition(Vector2 position, Vector2 dir)
    {
        PlayerMovement pm = Player.Instance.Movement;
        pm.EndJump(true);
        if (pm.SpecialState != SpecialState.Chronoshift && pm.SpecialState != SpecialState.GroundSlam) pm.SpecialState = SpecialState.Normal;

        pm.transform.position = position;
    }

    // relative position from 0 to 1 in each axis, if in bounds
    public Vector2 GetRelativePosition(Room room, Vector2 absPos)
    {
        Vector2 unscaledRelPos = absPos - (Vector2)room.transform.position;
        Vector2 relPos = new (unscaledRelPos.x / (room.size.x * BaseWidth),
                              unscaledRelPos.y / (room.size.y * BaseHeight));
        return relPos;
    }

    public void FinalizeRooms()
    {
        AllRooms = FindObjectsByType<Room>(FindObjectsSortMode.InstanceID).ToList();

        foreach (Room room in AllRooms)
        {
#if UNITY_EDITOR
            Undo.RecordObject(room.transform, "Grid-align rooms");
#endif

            Vector3 roomPos = room.transform.position;
            roomPos.x = room.gridPosition.x * BaseWidth * 1.2f;
            roomPos.y = room.gridPosition.y * BaseHeight * 1.2f;
            room.transform.position = roomPos;

            foreach (var e in room.Entities)
            {
                e.OnValidate();
            }
        }
#if UNITY_EDITOR
        foreach (Passage passage in FindObjectsByType<Passage>(FindObjectsSortMode.None))
        {
            var pe = passage.GetComponent<PassageEditor>();
            if (pe != null)
                pe.FinalizePassage();
        }
#endif

    }

}
