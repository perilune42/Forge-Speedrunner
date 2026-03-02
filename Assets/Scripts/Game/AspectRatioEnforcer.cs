using UnityEngine;

public class AspectRatioEnforcer : MonoBehaviour
{
    public float targetAspect = 16f / 9f;

    private void Awake()
    {
        // Create a background camera that clears to black
        Camera bgCam = new GameObject("BackgroundCamera").AddComponent<Camera>();
        bgCam.depth = GetComponent<Camera>().depth - 1;
        bgCam.clearFlags = CameraClearFlags.SolidColor;
        bgCam.backgroundColor = Color.black;
        bgCam.cullingMask = 0; // Render nothing
        bgCam.rect = new Rect(0, 0, 1, 1); // Full screen
    }

    void Update()
    {
        var variance = targetAspect / ((float)Screen.width / Screen.height);
        if (variance < 1.0)
            Camera.main.rect = new Rect((1f - variance) / 2f, 0, variance, 1f);
        else
        {
            variance = 1f / variance;
            Camera.main.rect = new Rect(0, (1f - variance) / 2f, 1f, variance);
        }
    }
}