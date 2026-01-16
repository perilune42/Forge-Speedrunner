using UnityEngine;
using System.Collections.Generic;

public class RoomSOManager : Singleton<RoomSOManager>
{
    /* switch rooms on and off(?)
     * move camera to be in the appropriate room
     * provide function to switch rooms
     */
    public Room activeRoom;
    private GameObject currentExistingRoom;

    void Start()
    {
        currentExistingRoom = Instantiate(activeRoom.RoomPrefab, activeRoom.RoomPrefab.transform.position, Quaternion.identity);
        printRoomStatus();
    }

    void Update() 
    {
        Room newRoom = null;
        if (Input.GetKeyDown(KeyCode.UpArrow)) 
        {
            newRoom = activeRoom.up;
            Debug.Log("up");
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) 
        {
            newRoom = activeRoom.down;
            Debug.Log("down");
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) 
        {
            newRoom = activeRoom.left;
            Debug.Log("left");
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) 
        {
            newRoom = activeRoom.right;
            Debug.Log("right");
        }
        if (newRoom != null) 
        {
            Destroy(currentExistingRoom);
            currentExistingRoom = Instantiate(newRoom.RoomPrefab, newRoom.RoomPrefab.transform.position, Quaternion.identity);
            activeRoom = newRoom;
            printRoomStatus();
        }
    }
    private void printRoomStatus() 
    {
        Debug.LogFormat("UP: {0}, DOWN: {1}, LEFT: {2}, RIGHT: {3}",
                activeRoom.up != null, activeRoom.down != null,
                activeRoom.left != null, activeRoom.right != null);
    }
}
