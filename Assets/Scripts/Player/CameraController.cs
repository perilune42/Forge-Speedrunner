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
        CameraBounds.size = new Vector2(room.size.x * RoomManager.Instance.BaseWidth, room.size.y * RoomManager.Instance.BaseHeight);
        CameraBounds.offset = CameraBounds.size / 2;
        CameraConfiner.InvalidateBoundingShapeCache();
        CinemachineCam.PreviousStateIsValid = false;
    }

}
