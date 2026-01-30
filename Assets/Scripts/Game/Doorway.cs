using UnityEngine;

public class Doorway : MonoBehaviour
{
    [HideInInspector] public Room enclosingRoom;
    [HideInInspector] public Passage passage;

    private bool suppressTransition = false;

    void Start()
    {
        enclosingRoom = GetComponentInParent<Room>();
        GenerateGuideRails();
        GenerateBlocker();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerMovement>() == null || suppressTransition) return;
        var otherDoor = this == passage.door1 ? passage.door2 : passage.door1;
        RoomManager.Instance.SwitchRoom(this, otherDoor);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerMovement>() != null)
        {
            Debug.Log("you'd normally un-suppress transitions here.");
            // suppressTransition = false;
        }
    }

    // true if this is a L/R transition
    public bool IsHorizontal()
    {
        return enclosingRoom.doorwaysLeft.Contains(this) || enclosingRoom.doorwaysRight.Contains(this);
    }

    public Vector2 GetTransitionDirection()
    {
        if (enclosingRoom.doorwaysLeft.Contains(this)) return Vector2.left;
        else if (enclosingRoom.doorwaysRight.Contains(this)) return Vector2.right;
        else if (enclosingRoom.doorwaysUp.Contains(this)) return Vector2.up;
        else if (enclosingRoom.doorwaysDown.Contains(this)) return Vector2.down;
        return Vector2.zero;
    }

    // suppress one activation until player exits
    public void SuppressNextTransition()
    {
        suppressTransition = true;
        // in case the trigger exit wasn't detected
        // StartCoroutine(Util.FDelayedCall(30, () => suppressTransition = false));
    }
    public void EnableTransition()
    {
        suppressTransition = false;
    }

    private void GenerateGuideRails()
    {
        Vector2 dir = GetTransitionDirection();
        for (int i = 0; i < 2; i++)
        {
            BoxCollider2D rail = Instantiate(RoomManager.Instance.GuideRailPrefab, transform);
            rail.transform.localPosition = Vector2.zero;
            rail.size = Vector2.one + 2 * new Vector2(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
            rail.offset = dir + 2.5f * Vector2.Perpendicular(dir) * (i == 0 ? 1 : -1);
        }
        BoxCollider2D freezeTrigger = Instantiate(RoomManager.Instance.FreezeTriggerPrefab, transform);
        freezeTrigger.transform.localPosition = Vector2.zero;
        freezeTrigger.size = Vector2.one + 3 * new Vector2(Mathf.Abs(dir.y), Mathf.Abs(dir.x));
        freezeTrigger.offset = dir * 2;
    }

    private void GenerateBlocker()
    {
        if (passage == null)
        {
            BoxCollider2D blocker;
            blocker = Instantiate(RoomManager.Instance.BlockerVertPrefab);
            // if (IsHorizontal())
            // {
            //     blocker = Instantiate(RoomManager.Instance.BlockerHorzPrefab);
            // }
            // else
            // {
            //     blocker = Instantiate(RoomManager.Instance.BlockerVertPrefab);
            // }
            blocker.transform.SetParent(transform, false);
            blocker.transform.position = transform.position - (Vector3)GetTransitionDirection();
        }
    }
}
