using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FullscreenMapUI : MonoBehaviour
{
    private List<Room> allRooms;
    private RoomManager roomManager;
    private static Vector2 screenRes;
    private int width;
    private int height;
    private float maxPosXY;
    private float minResXY;
    
    [SerializeField] private Object roomImage;
    [SerializeField] [Range(0.1f, 1)] private float sizeMult = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        screenRes = new Vector2(Screen.width, Screen.height);
        StartCoroutine(produceImages());
    }

    IEnumerator produceImages()
    {
        yield return new WaitForSeconds(0.05f);
        roomManager = RoomManager.Instance;
        allRooms = roomManager.AllRooms;
        width = roomManager.BaseWidth;
        height = roomManager.BaseHeight;
        FindMaxXY();
        FindMinResXY();
        float unitX = sizeMult * minResXY/(maxPosXY * 2);
        float unitY = unitX * height/width;
        
        foreach (Room room in allRooms)
        {
            Object image = Instantiate(roomImage, transform);
            Vector2 relativePos = new Vector2(room.gridPosition.x * unitX - unitX/2, 
                room.gridPosition.y * unitY - unitY/2);
            Vector2 relativeSize = new Vector2(room.size.x * unitX, 
                room.size.y * unitY);
            image.GetComponent<RectTransform>().localPosition = 
                relativePos;
            image.GetComponent<RectTransform>().sizeDelta = 
                relativeSize;
        }
    }

    // Finding the max X and Y of the grid positions of the room
    private void FindMaxXY()
    {
        maxPosXY = 1; // Defaulted at 1
        foreach (Room room in allRooms)
        {
            int x = Mathf.Abs(room.gridPosition.x);
            int y = Mathf.Abs(room.gridPosition.y);
            // Add one if positive x, y because the grid location is based on bottom left corner
            int fixedX = (x > 0) ? (x + 1) : x; 
            int fixedY = (y > 0) ? (y + 1) : y; 
            if (fixedX > maxPosXY)
            {
                maxPosXY = fixedX;
            }
            if (fixedY > maxPosXY)
            {
                maxPosXY = fixedY;
            }
        }
    }

    // Finding the smaller between Screen resolutions height and width
    // This will be used to find a unit for the UI map elements
    private void FindMinResXY()
    {
        minResXY = (screenRes.x < screenRes.y) ? screenRes.x : screenRes.y;
    }
}
