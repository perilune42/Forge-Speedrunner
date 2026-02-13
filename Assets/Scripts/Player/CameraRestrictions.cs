using UnityEngine;
using Unity.Cinemachine;

[SaveDuringPlay]
[AddComponentMenu("")] // Hide in menu
public class CameraRestrictions : CinemachineExtension
{
    [Tooltip("Lock the camera's X position to this value")]
    public float xPosition = 0;
    public bool xLocked = false;
    [Tooltip("Lock the camera's Y position to this value")]
    public float yPosition = 0;
    public bool yLocked = false;

    public Vector2 BoundsPos;
    public Vector2 BoundsSize;
    public bool xBound = false;
    public bool yBound = false;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        Camera cam = Camera.main;

        if (stage == CinemachineCore.Stage.Body)
        {
            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            Vector3 pos = state.RawPosition;
            Vector3 posCorr = state.PositionCorrection;

            if (xBound)
            {
                float newX = Mathf.Clamp(pos.x, BoundsPos.x + camWidth, BoundsPos.x + BoundsSize.x - camWidth);
                posCorr.x = newX != pos.x ? 0.0f : posCorr.x;
                pos.x = newX;
            }

            if (yBound)
            {
                float newY = Mathf.Clamp(pos.y, BoundsPos.y + camHeight, BoundsPos.y + BoundsSize.y - camHeight);
                posCorr.y = newY != pos.y ? 0.0f : posCorr.y;
                pos.y = newY;
            }
            state.RawPosition = pos;
            state.PositionCorrection = posCorr;
        }

        if (stage == CinemachineCore.Stage.Finalize)
        {
            var pos = state.RawPosition;
            var posCorr = state.PositionCorrection;
            if (xLocked)
            {
                pos.x = xPosition;
                posCorr.x = 0.0f;
            }

            if (yLocked)
            {
                pos.y = yPosition;
                posCorr.y = 0.0f;
            }
            state.RawPosition = pos;
            state.PositionCorrection = posCorr;
        }
    }

    public void LockXAxis(float xPos)
    {
        xPosition = xPos;
        xLocked = true;
    }

    public void UnlockXAxis()
    {
        xLocked = false;
    }

    public void LockYAxis(float yPos)
    {
        yPosition = yPos;
        yLocked = true;
    }

    public void UnlockYAxis()
    {
        yLocked = false;
    }

    public void BoundX(Vector2 boundsPos, Vector2 boundsSize)
    {
        xBound = true;
        BoundsPos = boundsPos;
        BoundsSize = boundsSize;
    }

    public void BoundY(Vector2 boundsPos, Vector2 boundsSize)
    {
        yBound = true;
        BoundsPos = boundsPos;
        BoundsSize = boundsSize;
    }

    public void UnboundX()
    {
        xBound = false;
    }

    public void UnboundY()
    {
        yBound = false;
    }
}