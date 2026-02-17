using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FullscreenMapUI : MonoBehaviour
{
    private List<Room> allRooms;
    private RoomManager roomManager;
    private static Vector2 screenRes;
    private int width;
    private int height;
    private float maxPosX;
    private float maxPosY;
    private Passage[] allPassages;
    private Color panelColor;
    private Color newColor;

    [SerializeField] private Vector2Int passageSize = new Vector2Int(20, 10); // In pixels
    [SerializeField] private Vector2Int roomSizeMinus = new Vector2Int(8, 8); // Remove some unitX and unitY to show borders
    [SerializeField] private Vector2Int youAreHereSize = new Vector2Int(2, 2); // In pixels
    [SerializeField] private bool negativePosRooms = false; // Are there rooms on the left side of the starting room too?
    [SerializeField] private Object roomImage;
    [SerializeField] private Object youAreHereImage;
    [SerializeField] private Object passageImage; // size 2x2
    [SerializeField] [Range(0.1f, 1)] private float sizeMult = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // screenRes = new Vector2(Screen.width, Screen.height);
        screenRes = transform.parent.GetComponent<RectTransform>().sizeDelta;
        panelColor = gameObject.GetComponent<Image>().color;
        newColor = panelColor;
        toggleMap(); // don't show the panel in the beginning
    }

    private void FixedUpdate()
    {
        if (PInput.Instance.Map.StoppedPressing)
        {
            if (toggleMap()) {
                Debug.Log("Produced");
                produceImages();
            } else {
                Debug.Log("Cleared");
                clearImages();
            }
        }
    }

    private void clearImages()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void produceImages()
    {

        roomManager = RoomManager.Instance;
        allPassages = roomManager.AllPassages;
        Debug.Log("PAssages: " + allPassages.Length);
        allRooms = roomManager.AllRooms;
        width = roomManager.BaseWidth;
        height = roomManager.BaseHeight;
        FindMaxXY();

        Vector2 divided = screenRes / new Vector2 (maxPosX, maxPosY);
        float unitX = sizeMult * (Mathf.Min(divided.x, divided.y) / 2);
        float unitY = unitX * height/width;
        float offsetX = (negativePosRooms) ? (0) : (unitX * maxPosX - unitX*roomSizeMinus.x/(width));
        float offsetY = (negativePosRooms) ? (0) : (unitY * maxPosY - unitY*roomSizeMinus.y/(height));
        if (!negativePosRooms) {
            unitX *= 2;
            unitY *= 2;
        }
        
        foreach (Room room in allRooms)
        {
            Object roomObj = Instantiate(roomImage, transform);
            Vector2 relativePos = new Vector2(room.gridPosition.x * unitX - offsetX, 
                room.gridPosition.y * unitY - offsetY);
            Vector2 relativeSize = new Vector2(room.size.x * unitX - unitX*roomSizeMinus.x/width, 
                room.size.y * unitY - unitY*roomSizeMinus.y/height);
            RectTransform roomRect = roomObj.GetComponent<RectTransform>();
            roomRect.localPosition = relativePos;
            roomRect.sizeDelta = relativeSize;
            roomRect.SetAsFirstSibling();
            
            if (roomManager.activeRoom == room)
            {
                Object youAreHere = Instantiate(youAreHereImage, roomRect);
                youAreHere.GetComponent<RectTransform>().sizeDelta = 
                    new Vector2(relativeSize.x/(width*room.size.x) * youAreHereSize.x, 
                    relativeSize.y/(height*room.size.y) * youAreHereSize.y);
            }

            foreach (Doorway door in GetDoors(room))
            {
                float x = door.transform.localPosition.x;
                float y = door.transform.localPosition.y;
                bool show = false;

                // Centering the doors before instantiating them (when pivot is bottom left)
                if ((x >= -0.5 && x <= 0) || (y >= -0.5 && y <= 0)) // Left or Down Door
                {
                    show = true;
                }

                if (show) 
                {
                    Object passageObj = Instantiate(passageImage, transform);
                    Vector2 relPos = new Vector2(relativePos.x + (x/width)*unitX, relativePos.y + (y/height)*unitY);
                    RectTransform passageRect = passageObj.GetComponent<RectTransform>();
                    passageRect.localPosition = relPos;
                    passageRect.sizeDelta = 
                        new Vector2(relativeSize.x/(width*room.size.x) * passageSize.x, 
                        relativeSize.y/(height*room.size.y) * passageSize.y);
                    if (y >= -0.5 && y <= 0)
                    {
                        passageRect.Rotate(0, 0, 90f); 
                    }
                }          
            }
        }
    }

    // Finding all passages for that room
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
        maxPosX = 1; // Defaulted at 1
        maxPosY = 1; // Defaulted at 1
        foreach (Room room in allRooms)
        {
            int x = room.gridPosition.x;
            int y = room.gridPosition.y;
            // Add one if positive x, y because the grid location is based on bottom left corner
            int fixedX = (x > 0) ? (Mathf.Abs(x) + room.size.x) : Mathf.Abs(x); 
            int fixedY = (y > 0) ? (Mathf.Abs(y) + room.size.y) : Mathf.Abs(y); 
            if (fixedX > maxPosX)
            {
                maxPosX = fixedX;
            }
            if (fixedY > maxPosY)
            {
                maxPosY = fixedY;
            }
        }
    }

    // toggles map panel
    private bool toggleMap() {
        if (newColor == panelColor) {
            newColor.a = 0f;
            gameObject.GetComponent<Image>().color = newColor;
            return false;
        } else {
            newColor = panelColor;
            gameObject.GetComponent<Image>().color = newColor;
            return true;
        }
    }
}
