using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public int Layer;
    Transform cameraTransform => Camera.main.transform;
    float spriteWidth;

    [SerializeField] SpriteRenderer baseTile;
    [SerializeField] int tilesOnEachSide = 1;
    [SerializeField] float parallaxFactor = 1f;
    [SerializeField] float xSpeed = 0f;
    [SerializeField] SpriteRenderer[] tiles;
    float tileWidth;
    float timeOffset;

    private void OnValidate()
    {
        baseTile.sortingOrder = Layer;
        if (Camera.main != null)
        {
            transform.position = (Vector2)Camera.main.transform.position;
        }
        
    }

    private void Awake()
    {
        baseTile.sortingOrder = Layer;
        spriteWidth = baseTile.bounds.size.x;
        tileWidth = baseTile.bounds.size.x;

        int total = tilesOnEachSide * 2 + 1;
        tiles = new SpriteRenderer[total];

        for (int i = 0; i < total; i++)
        {
            SpriteRenderer t = Instantiate(baseTile, transform);
            t.transform.localPosition = Vector3.right * (i - tilesOnEachSide) * tileWidth;
            tiles[i] = t;
        }

        Destroy(baseTile.gameObject);
    }
    void FixedUpdate()
    {
        timeOffset += xSpeed * Time.fixedDeltaTime;
        timeOffset = Util.RepeatSigned(timeOffset, 1.5f * tilesOnEachSide * tileWidth);
        transform.position = (Vector2)cameraTransform.position;
        for (int i = 0; i < tiles.Length; i++)
        {
            int n = i - tilesOnEachSide;
            float xOffset = -transform.position.x * parallaxFactor + timeOffset;
            xOffset = Util.RepeatSigned(xOffset + n * tileWidth, 1.5f * tilesOnEachSide * tileWidth);
            tiles[i].transform.localPosition = new Vector2(xOffset, 0);
        }
    }
}
