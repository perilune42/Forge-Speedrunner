using UnityEngine;
using System.Collections.Generic;

public enum Edge
{
    NONE,
    LEFT,
    RIGHT,
    UP,
    DOWN
}

public class RoomGraphManager : Singleton<RoomGraphManager>
{
    /* switch rooms on and off(?)
     * move camera to be in the appropriate room
     * provide function to switch rooms
     */
    public int activeRoomId;
    public List<GameObject> RoomPrefabs;
    public List<List<Edge>> RoomGraph;
    public List<Edge> RoomGraphFlat;
    private GameObject currentExistingRoom;

    void Start()
    {
        GameObject activeRoom = RoomPrefabs[activeRoomId];
        currentExistingRoom = Instantiate(activeRoom, activeRoom.transform.position, Quaternion.identity);
        printRoomStatus();
    }

    void Update() 
    {
        Edge direction = Edge.NONE;
        if (Input.GetKeyDown(KeyCode.UpArrow)) 
        {
            direction = Edge.UP;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) 
        {
            direction = Edge.DOWN;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) 
        {
            direction = Edge.LEFT;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) 
        {
            direction = Edge.RIGHT;
        }
        if (direction == Edge.NONE)
        {
            return;
        }
        Debug.LogFormat("DIRECTION: {0}", direction);

        for(int i = 0; i < RoomPrefabs.Count; i++)
        {
            Edge curr = RoomGraphFlat[activeRoomId * RoomPrefabs.Count + i];
            if (curr == direction)
            {
                GameObject newRoom = RoomPrefabs[i];
                activeRoomId = i;
                Destroy(currentExistingRoom);
                currentExistingRoom = Instantiate(newRoom, newRoom.transform.position, Quaternion.identity);
                printRoomStatus();
                break;
            }
        }
    }
    private void printRoomStatus() 
    {
        int start = activeRoomId * RoomPrefabs.Count;
        Debug.LogFormat("[{0}, {1}, {2}, {3}]",
                RoomGraphFlat[start], RoomGraphFlat[start+1], RoomGraphFlat[start+2], RoomGraphFlat[start+3]);
    }
}
