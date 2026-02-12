using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;

public class CameraController : Singleton<CameraController>
{
    public Camera Cam;
    public CinemachineCamera CinemachineCam;
    public BoxCollider2D CameraBounds;
    public CinemachineConfiner2D CameraConfiner;
    public LockCameraAxis LockCamAxis;


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

        LockCameraAxis lockCamAxis = CinemachineCam.GetComponent<LockCameraAxis>();

        if (room.size.x == 1 && room.size.y == 1)
        {
            CameraConfiner.enabled = false;
            lockCamAxis.LockXAxis(room.transform.position.x + xSize / 2);
            lockCamAxis.LockYAxis(room.transform.position.y + ySize / 2);
        }
        else if (room.size.x == 1)
        {
            CameraConfiner.enabled = true;
            lockCamAxis.LockXAxis(room.transform.position.x + xSize / 2);
            lockCamAxis.UnlockYAxis();
            CameraBounds.size = new Vector2(xSize + (float)0.0001, ySize);
            CameraBounds.offset = CameraBounds.size / 2;
        }
        else if (room.size.y == 1)
        {
            CameraConfiner.enabled = true;
            lockCamAxis.LockYAxis(room.transform.position.y + ySize / 2);
            lockCamAxis.UnlockXAxis();
            CameraBounds.size = new Vector2(xSize, ySize + (float)0.0001);
            CameraBounds.offset = CameraBounds.size / 2;
        }
        else
        {
            CameraConfiner.enabled = true;
            lockCamAxis.UnlockXAxis();
            lockCamAxis.UnlockYAxis();
            CameraBounds.size = new Vector2(xSize, ySize);
            CameraBounds.offset = CameraBounds.size / 2;
        }

        CameraConfiner.InvalidateBoundingShapeCache();
        CinemachineCam.PreviousStateIsValid = false;
    }

}
