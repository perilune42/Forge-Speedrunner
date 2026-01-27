using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
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
    private Passage[] allPassages;

    [SerializeField] private Vector2Int passageSize = new Vector2Int(20, 10); // In pixels
    [SerializeField] private Vector2Int roomSizeMinus = new Vector2Int(8, 8); // Remove some unitX and unitY to show borders 
    [SerializeField] private Object roomImage;
    [SerializeField] private Object passageImage; // size 2x2
    [SerializeField] [Range(0.1f, 1)] private float sizeMult = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        screenRes = new Vector2(Screen.width, Screen.height);

        // A delay so that allPasssages can be populated first
        StartCoroutine(produceImages());
    }

    IEnumerator produceImages()
    {
        yield return new WaitForSeconds(0.05f);

        roomManager = RoomManager.Instance;
        allPassages = roomManager.AllPassages;
        allRooms = roomManager.AllRooms;
        width = roomManager.BaseWidth;
        height = roomManager.BaseHeight;
        FindMaxXY();
        FindMinResXY();
        float unitX = sizeMult * minResXY/(maxPosXY * 2);
        float unitY = unitX * height/width;
        
        foreach (Room room in allRooms)
        {
            Object roomObj = Instantiate(roomImage, transform);
            Vector2 relativePos = new Vector2(room.gridPosition.x * unitX - unitX/2, 
                room.gridPosition.y * unitY - unitY/2);
            Vector2 relativeSize = new Vector2(room.size.x * unitX - unitX*roomSizeMinus.x/width, 
                room.size.y * unitY - unitY*roomSizeMinus.y/height);
            RectTransform roomRect = roomObj.GetComponent<RectTransform>();
            roomRect.localPosition = relativePos;
            roomRect.sizeDelta = relativeSize;
            roomRect.SetAsFirstSibling();
            

            foreach (Doorway door in GetDoors(room))
            {
                float x = door.transform.localPosition.x;
                float y = door.transform.localPosition.y;
                bool show = false;

                // Choosing whether to show the passage (only if left or down)
                if (x == 0 || x == -0.5) // Left
                {
                    show = true;
                    y -= roomSizeMinus.y/2;                    
                } else if (y ==0 || y == -0.5) // Down
                {
                    show = true;
                    x -= roomSizeMinus.x/2;
                }

                if (show) 
                {
                    Object passageObj = Instantiate(passageImage, transform);
                    Vector2 relPos = new Vector2(relativePos.x + (x/width)*unitX, 
                        relativePos.y + (y/height)*unitY);
                    RectTransform passageRect = passageObj.GetComponent<RectTransform>();
                    passageRect.localPosition = relPos;
                    passageRect.sizeDelta = 
                        new Vector2(relativeSize.x/(width*room.size.x) * passageSize.x, 
                        relativeSize.y/(height*room.size.y) * passageSize.y);
                    if (y == 0)
                    {
                        passageRect.Rotate(0, 0, 90f); 
                    }
                }          
            }
        }
    }

    // Finding all doors for that room
    private List<Doorway> GetDoors(Room room)
    {
        List<Doorway> doors = new List<Doorway>();
        foreach (Passage pass in allPassages)
        {
            Room room1 = pass.door1.enclosingRoom;
            Room room2 = pass.door2.enclosingRoom;
            if (room1 == room)
            {
                doors.Add(pass.door1);
            } else if (room2 == room)
            {
                doors.Add(pass.door2);
            }
        }

        return doors;
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
