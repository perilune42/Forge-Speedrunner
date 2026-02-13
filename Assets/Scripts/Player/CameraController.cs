using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;

public class CameraController : Singleton<CameraController>
{
    public Camera Cam;
    public CinemachineCamera CinemachineCam;
    public BoxCollider2D CameraBounds;
    public CinemachineConfiner2D CameraConfiner;


    public override void Awake()
    {
        base.Awake();
    }

    public void SnapToRoom(Room room)
    {
        CameraBounds.transform.position = room.transform.position;
        float xSize = room.size.x * RoomManager.Instance.BaseWidth;
        float ySize = room.size.y * RoomManager.Instance.BaseHeight;
        //CameraBounds.size = new Vector2(room.size.x * RoomManager.Instance.BaseWidth, room.size.y * RoomManager.Instance.BaseHeight);
        //CameraBounds.offset = CameraBounds.size / 2;

        CameraRestrictions camRestrict= CinemachineCam.GetComponent<CameraRestrictions>();

        if (room.size.x == 1 && room.size.y == 1)
        {
            CameraConfiner.enabled = false;
            camRestrict.LockXAxis(room.transform.position.x + xSize / 2);
            camRestrict.LockYAxis(room.transform.position.y + ySize / 2);
            camRestrict.UnboundX();
            camRestrict.UnboundY();
        }
        else if (room.size.x == 1)
        {
            CameraConfiner.enabled = false;
            camRestrict.LockXAxis(room.transform.position.x + xSize / 2);
            camRestrict.UnlockYAxis();
            
            CameraBounds.size = new Vector2(xSize + (float)0.0001, ySize);
            CameraBounds.offset = CameraBounds.size / 2;
            camRestrict.BoundY(CameraBounds.transform.position, CameraBounds.size);
            camRestrict.UnboundX();
        }
        else if (room.size.y == 1)
        {
            CameraConfiner.enabled = false;
            camRestrict.LockYAxis(room.transform.position.y + ySize / 2);
            camRestrict.UnlockXAxis();

            CameraBounds.size = new Vector2(xSize, ySize + (float)0.0001);
            CameraBounds.offset = CameraBounds.size / 2;
            camRestrict.BoundX(CameraBounds.transform.position, CameraBounds.size);
            camRestrict.UnboundY();
        }
        else
        {
            CameraConfiner.enabled = true;
            camRestrict.UnlockXAxis();
            camRestrict.UnlockYAxis();
            CameraBounds.size = new Vector2(xSize, ySize);
            CameraBounds.offset = CameraBounds.size / 2;

            // If we want to remove confiner
            //camRestrict.BoundX(CameraBounds.transform.position, CameraBounds.size);
            //camRestrict.BoundY(CameraBounds.transform.position, CameraBounds.size);
        }

        CameraConfiner.InvalidateBoundingShapeCache();
        CinemachineCam.PreviousStateIsValid = false;
    }

}
