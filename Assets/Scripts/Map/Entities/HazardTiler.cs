using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class HazardTiler : MonoBehaviour
{
    private Hazard hazard;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxColl;
    public Vector2 DimensionsXY = new(1,1);

    public void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        hazard = GetComponent<Hazard>();
        boxColl = GetComponent<BoxCollider2D>();
        if(spriteRenderer == null || hazard == null || boxColl == null)
        {
            // Debug.Log($"Failed to initialize HazardTiler! Make sure there's a BoxCollider ({boxColl}), SpriteRenderer ({spriteRenderer}), and Hazard ({hazard}).");
            return;
        }
        spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        boxColl.autoTiling = true;
    }

    private void OnValidate()
    {
        // new size calculation
        Rect dim = spriteRenderer.sprite.rect;
        float unitConversion = spriteRenderer.sprite.pixelsPerUnit;
        Vector2 newSize =  new Vector2(dim.width, dim.height);
        newSize *= DimensionsXY;
        newSize /= unitConversion;

        // set size safely
        Undo.RecordObject(spriteRenderer, "OnValidate resize in editor");
        spriteRenderer.size = newSize;
        EditorUtility.SetDirty(spriteRenderer);
    }

    public void Update()
    {
        // in this situation, not good to run
        if(Application.isPlaying || !transform.hasChanged) return;
    }
}
