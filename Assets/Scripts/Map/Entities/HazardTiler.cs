using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[ExecuteAlways]
public class HazardTiler : MonoBehaviour
{
    private Hazard hazard;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxColl;
    private bool valueChanged = false;
    public Vector2Int DimensionsXY = new(1,1);

    public Vector2 colOffset;

    public void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        hazard = GetComponent<Hazard>();
        boxColl = GetComponent<BoxCollider2D>();
        if(spriteRenderer == null || hazard == null || boxColl == null)
        {
            // Debug.Log($"Failed to initialize HazardTiler! Make sure there's a BoxCollider ({boxColl}), SpriteRenderer ({spriteRenderer}), and Hazard ({hazard}).");
            return;
        }
        spriteRenderer.drawMode = SpriteDrawMode.Tiled;
        // boxColl.autoTiling = true;
    }

    private void OnValidate()
    {
        valueChanged = true;
    }

    public void Update()
    {
        // guards to prevent running at wrong time
        if(Application.isPlaying || !transform.hasChanged) return;
        if(!valueChanged) return;

        // new size calculation
        // Rect dim = spriteRenderer.sprite.rect;
        float unitConversion = spriteRenderer.sprite.pixelsPerUnit;
        Vector2 newSize =  Vector2.one;
        newSize *= DimensionsXY;
        // newSize /= unitConversion;

        // set size safely
        boxColl.size = new Vector2(newSize.x, boxColl.size.y);
        boxColl.offset = colOffset;

        Undo.RecordObject(spriteRenderer, "OnValidate resize in editor");
        spriteRenderer.size = newSize;
        EditorUtility.SetDirty(spriteRenderer);

        // update done
        valueChanged = false;
    }
}
#endif
