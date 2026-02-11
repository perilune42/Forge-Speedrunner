using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Cryptography;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class PlatformTiler : MonoBehaviour
{
    [SerializeField] SpriteRenderer srPrefab;
    List<SpriteRenderer> spriteRenderers = new();
    public List<Sprite> LSprites;
    public List<Sprite> MSprites;
    public List<Sprite> RSprites;

    private BoxCollider2D boxColl;
    private bool valueChanged = false;
    public int length;
    public Vector2 colOffset;
    public Vector2 spriteOffset;

    public bool capLeft, capRight;



    public void Start()
    {
        boxColl = GetComponent<BoxCollider2D>();
        // boxColl.autoTiling = true;
    }

    private void OnValidate()
    {
        valueChanged = true;
    }

    public void Update()
    {
        if (!enabled) return;

        // guards to prevent running at wrong time
        if(Application.isPlaying || !transform.hasChanged) return;
        if(!valueChanged) return;

        if (length < 1) return;

        if (spriteRenderers == null)
        {
            spriteRenderers = new();
        }

        Undo.RecordObject(transform, "OnValidate resize in editor");

        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            DestroyImmediate(sr.gameObject);
        }

        spriteRenderers.Clear();
        for (int i = 0; i < length; i++)
        {
            spriteRenderers.Add(Instantiate(srPrefab, transform));
        }

        if (length == 1)
        {
            spriteRenderers[0].sprite = MSprites.GetRandom();
            SetSrPosition(spriteRenderers[0], 0);
        }
        else
        {
            spriteRenderers[0].sprite = (capLeft ? LSprites : MSprites).GetRandom();
            SetSrPosition(spriteRenderers[0], 0);
            for (int i = 1; i < length - 1; i++)
            {
                spriteRenderers[i].sprite = MSprites.GetRandom();
                SetSrPosition(spriteRenderers[i], i);
            }
            spriteRenderers[length - 1].sprite = (capRight ? RSprites : MSprites).GetRandom();
            SetSrPosition(spriteRenderers[length - 1], length - 1);
        }


        // new size calculation
        // Rect dim = spriteRenderer.sprite.rect;
        Vector2 newSize =  Vector2.one;
        newSize.x *= length;
        // set size safely
        boxColl.size = new Vector2(newSize.x, boxColl.size.y);
        boxColl.offset = colOffset + Vector2.right * length / 2f;
        EditorUtility.SetDirty(transform);



        // update done
        valueChanged = false;
    }

    private void SetSrPosition(SpriteRenderer sr, int pos)
    {
        sr.transform.localPosition = new Vector2(pos, 0) + spriteOffset;
        EditorUtility.SetDirty(sr);
    }
}
#endif
