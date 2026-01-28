using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public int Layer;
    private void OnValidate()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = Layer;
        if (Camera.main != null)
        {
            transform.position = (Vector2)Camera.main.transform.position;
        }
        
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = Layer;
    }
    private void Update()
    {
        // no longer necessary - set as child of camera
        // transform.position = (Vector2)Camera.main.transform.position;
    }
}
