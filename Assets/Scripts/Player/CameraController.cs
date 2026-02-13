using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;

public class CameraController : Singleton<CameraController>
{
    public Camera Cam;
    public CinemachineCamera CinemachineCam;
    public BoxCollider2D CameraBounds;

    public override void Awake()
    {
        base.Awake();
    }

    public void SnapToRoom(Room room)
    {
        Debug.Log("snapping");
        CameraBounds.transform.position = room.transform.position;
        float xSize = room.size.x * RoomManager.Instance.BaseWidth;
        float ySize = room.size.y * RoomManager.Instance.BaseHeight;

        CameraRestrictions camRestrict = CinemachineCam.GetComponent<CameraRestrictions>();

        if (room.size.x == 1 && room.size.y == 1)
        {
            camRestrict.LockXAxis(room.transform.position.x + xSize / 2);
            camRestrict.LockYAxis(room.transform.position.y + ySize / 2);
        }
        else if (room.size.x == 1)
        {
            camRestrict.LockXAxis(room.transform.position.x + xSize / 2);
            camRestrict.UnlockYAxis();
        }
        else if (room.size.y == 1)
        {
            camRestrict.LockYAxis(room.transform.position.y + ySize / 2);
            camRestrict.UnlockXAxis();
        }
        else
        {
            camRestrict.UnlockXAxis();
            camRestrict.UnlockYAxis();
        }

        CameraBounds.size = new Vector2(xSize, ySize);
        CameraBounds.offset = CameraBounds.size / 2;
        camRestrict.SetBounds(CameraBounds.transform.position, CameraBounds.size);

        CinemachineCam.PreviousStateIsValid = false;
    }

}
