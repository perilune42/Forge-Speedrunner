using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HideWhenPlayerBehind : MonoBehaviour
{
    private Transform player;
    private Camera cam;
    [SerializeField] private float detectionExpandAmount = 50f; // Amount to expand detection area in pixels

    private Image[] images;
    private RectTransform rectTransform;

    void Start()
    {
        player = Player.Instance.transform;
        cam = CameraController.Instance.Cam;
        rectTransform = GetComponent<RectTransform>();
    }

    void FixedUpdate()
    {
        List<Image> allImages = new List<Image>();
        foreach (Image image in transform.GetComponentsInChildren<Image>())
        {
            allImages.Add(image);
        }
        images = allImages.ToArray();

        Vector2 playerScreenPos = cam.WorldToScreenPoint(player.position);

        // Get screen rect of the UI element
        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);
        Vector2[] screenCorners = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            screenCorners[i] = cam.WorldToScreenPoint(worldCorners[i]);
        }
        float minX = Mathf.Min(Mathf.Min(screenCorners[0].x, screenCorners[1].x), Mathf.Min(screenCorners[2].x, screenCorners[3].x));
        float maxX = Mathf.Max(Mathf.Max(screenCorners[0].x, screenCorners[1].x), Mathf.Max(screenCorners[2].x, screenCorners[3].x));
        float minY = Mathf.Min(Mathf.Min(screenCorners[0].y, screenCorners[1].y), Mathf.Min(screenCorners[2].y, screenCorners[3].y));
        float maxY = Mathf.Max(Mathf.Max(screenCorners[0].y, screenCorners[1].y), Mathf.Max(screenCorners[2].y, screenCorners[3].y));
        Rect screenRect = new Rect(minX, minY, maxX - minX, maxY - minY);

        // Expand the detection area
        screenRect.xMin -= detectionExpandAmount;
        screenRect.xMax += detectionExpandAmount;
        screenRect.yMin -= detectionExpandAmount;
        screenRect.yMax += detectionExpandAmount;

        bool playerIsBehind = screenRect.Contains(playerScreenPos);

        foreach (Image image in images)
        {
            if (playerIsBehind) {
                image.enabled = false;
            } else {
                image.enabled = true;
            }
        }
    }
}