using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class FullscreenMapUI : MonoBehaviour
{
    private List<Room> allRooms;
    private List<Room> visitedRooms = new List<Room>();
    private RoomManager roomManager;
    private Vector2 screenRes;
    private int width;
    private int height;
    private float maxPosX;
    private float maxPosY;
    private Color panelColor;
    private Color newColor;

    private bool showingMap;

    [SerializeField] private bool shopMode;


    [SerializeField] private Vector2Int passageSize = new Vector2Int(20, 10); // In pixels
    [SerializeField] private Vector2Int roomSizeMinus = new Vector2Int(8, 8); // Remove some unitX and unitY to show borders
    [SerializeField] private Vector2Int youAreHereSize = new Vector2Int(2, 2); // In pixels
    [SerializeField] private bool negativePosRooms = false; // Are there rooms on the left side of the starting room too?
    [SerializeField] private Object roomImage;
    [SerializeField] private Object youAreHereImage;
    [SerializeField] private Object passageImage; // size 2x2
    [SerializeField] [Range(0.1f, 1)] private float sizeMult = 1;

    bool initialized = false;

    [SerializeField] MapSpawnSelector mapSpawnSelectorPrefab, startSpawnSelectorPrefab;

    [SerializeField] private Transform roomContainer;
    [SerializeField] private Transform passageContainer;
    [SerializeField] private Transform spawnSelectorContainer;

    private Dictionary<Room, RectTransform> roomRects = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();
        if (!shopMode)
        {
            produceImages();
            toggleMap(false);
        }

    }

    private void Init()
    {
        screenRes = GameplayUI.Instance.GetComponent<RectTransform>().sizeDelta;
        panelColor = gameObject.GetComponent<Image>().color;
        newColor = panelColor;
        initialized = true;
    }

    private void FixedUpdate()
    {
        if (!shopMode && PInput.Instance.Map.StoppedPressing)
        {
            if (!showingMap)
            {
                clearImages();
                produceImages();
            }
            toggleMap(!showingMap);
        }

    }

    public void clearImages()
    {
        foreach (Transform child in roomContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in passageContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in spawnSelectorContainer)
        {
            Destroy(child.gameObject);
        }
        visitedRooms.Clear();
        roomRects.Clear();
    }

    public void produceImages()
    {
        if (!initialized) Init();  
        roomManager = RoomManager.Instance;
        allRooms = roomManager.AllRooms;

        width = roomManager.BaseWidth;
        height = roomManager.BaseHeight;
        FindMaxXY();

        Vector2 divided = screenRes / new Vector2 (maxPosX, maxPosY);
        float unitX = sizeMult * (Mathf.Min(divided.x, divided.y) / 2);
        float unitY = unitX * height/width;
        //float offsetX = (negativePosRooms) ? (0) : (unitX * maxPosX - unitX*roomSizeMinus.x/(width));
        //float offsetY = (negativePosRooms) ? (0) : (unitY * maxPosY - unitY*roomSizeMinus.y/(height));
        float offsetX = (negativePosRooms) ? (0) : (unitX * maxPosX);
        float offsetY = (negativePosRooms) ? (0) : (unitY * maxPosY);
        if (!negativePosRooms) {
            unitX *= 2;
            unitY *= 2;
        }

        // show only visited rooms and passages
        for (int i = allRooms.Count - 1; i >= 0; i--)
        {
            if (allRooms[i].visited)
            {
                visitedRooms.Add(allRooms[i]);
            }
        }
        
        foreach (Room room in visitedRooms)
        {
            Object roomObj = Instantiate(roomImage, transform);
            Vector2 relativePos = new Vector2(room.gridPosition.x * unitX - offsetX, 
                room.gridPosition.y * unitY - offsetY);
            Vector2 relativeSize = new Vector2(room.size.x * unitX - unitX*roomSizeMinus.x/width, 
                room.size.y * unitY - unitY*roomSizeMinus.y/height);
            RectTransform roomRect = roomObj.GetComponent<RectTransform>();
            roomRect.localPosition = relativePos;
            roomRect.sizeDelta = relativeSize;
            roomRect.localPosition += new Vector3(unitX * roomSizeMinus.x / (2 * width), unitY * roomSizeMinus.y / (2 * height));
            roomRect.transform.SetParent(roomContainer, true);
            roomRects[room] = roomRect;

            if (!shopMode && roomManager.activeRoom == room)
            {
                GameObject youAreHere = Instantiate(youAreHereImage, roomRect).GameObject();
                youAreHere.GetComponent<RectTransform>().sizeDelta = 
                    new Vector2(relativeSize.x/(width*room.size.x) * youAreHereSize.x, 
                    relativeSize.y/(height*room.size.y) * youAreHereSize.y);
                youAreHere.transform.SetParent(roomContainer, true);
            }
            foreach (Doorway door in GetDoors(room))
            {
                
                float x = door.transform.localPosition.x;
                float y = door.transform.localPosition.y;
                bool show = false;

                Vector2 dir = door.GetTransitionDirection();
                float xIdx, yIdx;
                if (dir.y == 0)
                {
                    // vertical
                    xIdx = dir.x == 1 ? room.size.x : 0;
                    yIdx = door.GetIndex() + 0.5f;
                }
                else
                {
                    // horizontal
                    xIdx = door.GetIndex() + 0.5f;
                    yIdx = dir.y == 1 ? room.size.y : 0;
                }

                if (dir == Vector2.left || dir == Vector2.down) // Left or Down Door
                {
                    show = true;
                }
                Vector2 relPos = new Vector2(relativePos.x + xIdx * unitX, relativePos.y + yIdx * unitY);
                if (show) 
                {
                    
                    GameObject passageObj = Instantiate(passageImage, transform).GameObject();
                    RectTransform passageRect = passageObj.GetComponent<RectTransform>();
                    passageRect.localPosition = relPos;
                    passageRect.sizeDelta = 
                        new Vector2(relativeSize.x/(width*room.size.x) * passageSize.x, 
                        relativeSize.y/(height*room.size.y) * passageSize.y);
                    if (dir == Vector2.down)
                    {
                        passageRect.Rotate(0, 0, 90f); 
                    }
                    passageObj.transform.SetParent(passageContainer, false);
                }
                
                if (shopMode)
                {
                    MapSpawnSelector mapSpawnSelector = Instantiate(mapSpawnSelectorPrefab, transform);
                    mapSpawnSelector.transform.localPosition = relPos + door.GetTransitionDirection() * -0.2f * unitX;
                    mapSpawnSelector.transform.SetParent(spawnSelectorContainer, true);
                    mapSpawnSelector.LinkToDoorway(door);
                    
                }

            }
        }
        if (shopMode) PlaceStartingSpawnSelector();
        ToggleSpawnSelectors(false);
    }

    // Finding all passages for that room
    private List<Doorway> GetDoors(Room room)
    {
        List<Doorway> doors = new List<Doorway>();
        doors.AddRange(room.doorwaysDown.Where((door) => door != null && door.passage != null && door.passage.visited));
        doors.AddRange(room.doorwaysLeft.Where((door) => door != null && door.passage != null && door.passage.visited));
        doors.AddRange(room.doorwaysRight.Where((door) => door != null && door.passage != null && door.passage.visited));
        doors.AddRange(room.doorwaysUp.Where((door) => door != null && door.passage != null && door.passage.visited));

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
    private bool toggleMap(bool toggle) {
        if (toggle) {
            GetComponent<Canvas>().enabled = true;
            showingMap = true;
            return true;
        } else {
            GetComponent<Canvas>().enabled = false;
            showingMap = false;
            return false;
        }
    }

    public void ToggleSpawnSelectors(bool toggle)
    {
        spawnSelectorContainer.gameObject.SetActive(toggle);
    }

    private void PlaceStartingSpawnSelector()
    {
        var roomRect = roomRects[RoomManager.Instance.StartingRoom];
        Vector2 roomRoot = roomRect.transform.position;
        MapSpawnSelector sel = Instantiate(startSpawnSelectorPrefab, transform);
        var relPos = RoomManager.Instance.GetRelativePosition(RoomManager.Instance.StartingRoom,
            RoomManager.Instance.StartingSpawn.transform.position);
        sel.transform.position = roomRoot + new Vector2(roomRect.rect.width * relPos.x, roomRect.rect.height * relPos.y);
        sel.transform.SetParent(spawnSelectorContainer, true);
    }
}
