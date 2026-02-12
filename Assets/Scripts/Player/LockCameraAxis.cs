using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// An add-on module for Cinemachine Virtual Camera that locks the camera's Y co-ordinate
/// </summary>
[SaveDuringPlay]
[AddComponentMenu("")] // Hide in menu
public class LockCameraAxis : CinemachineExtension
{
    [Tooltip("Lock the camera's X position to this value")]
    public float xPosition = 0;
    public bool xLocked = false;
    [Tooltip("Lock the camera's Y position to this value")]
    public float yPosition = 0;
    public bool yLocked = false;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
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
}