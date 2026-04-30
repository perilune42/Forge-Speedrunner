using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;

public class HideWhenPlayerBehind : MonoBehaviour
{
    private Transform player;
    private Camera cam;
    [SerializeField] private float detectionExpandAmount = 50f; // Amount to expand detection area in pixels

    private List<Image> images;
    private RectTransform rectTransform;
    Dictionary<Image, float> origAlpha = new();

    void Start()
    {
        player = Player.Instance.transform;
        cam = CameraController.Instance.Cam;
        rectTransform = GetComponent<RectTransform>();
        Game.Instance.OnEnterWorld += GetImages;
    }

    private void GetImages()
    {
        images = GetComponentsInChildren<Image>().ToList();
        images.AddRange(GetComponents<Image>());
        origAlpha = new();
        foreach (Image image in images)
        {
            origAlpha[image] = image.color.a;
        }
    }

    void FixedUpdate()
    {
        /*
        Vector2 playerScreenPos = cam.WorldToScreenPoint(player.position);

        

        // Get screen rect of the UI element
        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);
        Vector2[] screenCorners = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            screenCorners[i] = worldCorners[i];
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
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0.3f);
            } else {
                image.color = new Color(image.color.r, image.color.g, image.color.b, origAlpha[image]);
            }
        }
        */
    }
}